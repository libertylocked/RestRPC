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
	retChannel := make(chan *ResponseObject, 50)
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
	go deliverResponseRoutine(retChannel, c)

	for {
		// Read input message from client
		_, data, err := c.ReadMessage()
		if err != nil {
			log.Println("CWS:", err)
			break
		}
		var reqObject RequestObject
		err = json.Unmarshal(data, &reqObject)
		if err != nil {
			log.Println("CWS: Error unmarshalling message!", err, string(data))
		}

		// Attach requester's UID to the request input
		reqObject.RID = clientUID.String()
		// Clear the TID before enqueuing
		targetID := reqObject.TID
		reqObject.TID = ""

		reqChLock.RLock()
		reqChannel := reqChMap[targetID]
		reqChLock.RUnlock()
		select {
		case reqChannel <- &reqObject:
			log.Println("WSC: Sent:", reqObject)
		default:
			// Fails to send the input because component's chan is full, or its channel does not exist
			log.Println("CWS: Input channel unavailable. Discarding:", reqObject)
			// TODO: Server respond with an error message
		}

	}
}

func deliverResponseRoutine(retChannel chan *ResponseObject, c *websocket.Conn) {
	for {
		retMsg, ok := <-retChannel
		if ok {
			// A response object has arrived for this requester!
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
