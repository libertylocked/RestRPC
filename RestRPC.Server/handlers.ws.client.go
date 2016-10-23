package main

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/satori/go.uuid"
)

// Handles a websocket connection from a procedure caller
func handleClientWS(w http.ResponseWriter, r *http.Request) {
	c, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		// CWS stands for client WS
		log.Println("CWS:", err)
		return
	}

	// Generate a UID to identify this client
	clientUID := uuid.NewV4()
	// Make a channel to signal the output goroutine to stop
	stopOutputRoutine := make(chan bool, 1)
	defer func() {
		c.Close()
		// Signal the output routine to stop
		stopOutputRoutine <- true
		// Delete return channel for this client
		retChLock.Lock()
		delete(retChMap, clientUID)
		retChLock.Unlock()
		log.Println("CWS: Client disconnected:", r.RemoteAddr)
	}()
	log.Println("CWS: Client connected:", r.RemoteAddr, clientUID)

	// Create a return channel for this client
	// XXX: Hard code return channel size 50: if more than 50 outputs are received before dequeued to client,
	// outputs will be discarded
	retChannel := make(chan *ProcedureReturn, 50)
	retChLock.Lock()
	retChMap[clientUID] = retChannel
	retChLock.Unlock()

	// Start the output routine: listen on the return channel and send to client when outputs are received
	go func() {
		for {
			select {
			case retMsg := <-retChannel:
				// A return message has arrived!
				log.Println("CWS: Returning response to client:", clientUID)
				c.WriteJSON(retMsg)
			case <-stopOutputRoutine:
				return
			}
		}
	}()

	for {
		// Read input message from client
		_, data, err := c.ReadMessage()
		if err != nil {
			log.Println("CWS:", err)
			break
		}
		var input ClientMessage
		err = json.Unmarshal(data, &input)
		if err != nil {
			log.Println("CWS: Error unmarshalling message!", err, string(data))
		}

		// Attach requester's UID to the request input
		input.UID = clientUID
		// Get the target of the request, then clear it from the input so we don't send it to component
		targetID := input.Target
		input.Target = ""

		inChLock.RLock()
		inChannel := inChMap[targetID]
		inChLock.RUnlock()
		select {
		case inChannel <- &input:
			log.Println("WSC: Sent:", input)
		default:
			// Fails to send the input because component's chan is full, or its channel does not exist
			log.Println("CWS: Input channel unavailable. Discarding:", input)
			// TODO: Server respond with an error message
		}

	}
}
