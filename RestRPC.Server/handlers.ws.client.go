package main

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/gorilla/websocket"

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
	log.Println("CWS: Client connected:", r.RemoteAddr, clientUID)

	// Create a return channel for this client
	// XXX: Hard code return channel size 50: if more than 50 outputs are received before dequeued to client,
	// outputs will be discarded
	retChannel := make(chan *ProcedureReturn, 50)
	retChLock.Lock()
	retChMap[clientUID] = retChannel
	retChLock.Unlock()

	defer func() {
		c.Close()
		// Delete return channel for this client
		retChLock.Lock()
		close(retChMap[clientUID])
		delete(retChMap, clientUID)
		retChLock.Unlock()
		log.Println("CWS: Client disconnected:", r.RemoteAddr)
	}()

	// Start the output routine: listen on the return channel and send to client when outputs are received
	go deliverOutputsRoutine(retChannel, c)

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

func deliverOutputsRoutine(retChannel chan *ProcedureReturn, c *websocket.Conn) {
	for {
		retMsg, ok := <-retChannel
		if ok {
			// A return message has arrived!
			errWrite := c.WriteJSON(retMsg)
			if errWrite != nil {
				log.Println("CWS:", errWrite)
			}
			log.Println("CWS: Response returned:", retMsg)
		} else {
			log.Println("CWS: Return channel closed!")
			break
		}
	}
}
