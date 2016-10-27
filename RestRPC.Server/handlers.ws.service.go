package main

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/gorilla/websocket"
	"github.com/satori/go.uuid"
)

var upgrader = websocket.Upgrader{
	ReadBufferSize:  1024,
	WriteBufferSize: 1024,
}

func handleComponentWS(w http.ResponseWriter, r *http.Request) {
	c, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("WS:", err)
		return
	}

	log.Println("WS: Component connected:", r.RemoteAddr)

	// Create a channel for the service
	svcName := getComponentNameCookie(r)
	newChan := createInChannel(svcName)
	if newChan == nil {
		// XXX: Close connection if channel creation failed. Should respond with an error message
		c.Close()
	} else {
		// If channel is successfully created, start input delivery routine
		go deliverInputsRoutine(newChan, c)
		defer func() {
			// Close the channel and remove from in channel map
			close(newChan)
			delete(inChMap, svcName)
			log.Println("WS: In channel deleted:", svcName)
		}()
	}

	for {
		// Read message from component
		// This will block until a service message (return value, pulse, etc) is received.
		_, data, err := c.ReadMessage()
		if err != nil {
			log.Println("WS:", err)
			break
		}
		var msg ServiceMessage
		err = json.Unmarshal(data, &msg)
		if err != nil {
			log.Println("WS: Error unmarshalling message!", err, string(data))
		} else {
			processServiceMessage(msg, c, r)
		}
	}
}

func processServiceMessage(msg ServiceMessage, c *websocket.Conn, r *http.Request) {
	defer func() {
		if rec := recover(); rec != nil {
			log.Println("WS: Error processing service message:", msg, rec)
			return
		}
	}()

	// Read the header
	switch msg.Header {
	case "c":
		// A cache request
		// TODO: not implemented

	case "r", "":
		// A return value
		uid, _ := uuid.FromString(msg.UID)
		retMsg := msg.GetProcedureReturn()
		retChLock.RLock()
		retChannel := retChMap[uid]
		retChLock.RUnlock()
		select {
		case retChannel <- retMsg:
			log.Println("WS: Returned output for:", uid)
		default:
			// Fails if either the return channel is full, or the return channel has been deleted
			log.Println("WS: Return channel unavailable. Discarding:", msg)
		}

	default:
		log.Println("WS: Unknown service message format:", msg)
	}
}

// A routine to continue deliver input messages to service until channel is closed
func deliverInputsRoutine(inChannel chan *ClientMessage, c *websocket.Conn) {
	for {
		input, ok := <-inChannel
		if ok {
			errWrite := c.WriteJSON(input)
			if errWrite != nil {
				log.Println("WS:", errWrite)
			}
			log.Println("WS: Dequeued:", input.UID)
		} else {
			log.Println("WS: Input channel closed!")
			break
		}
	}
}

func getComponentNameCookie(r *http.Request) string {
	cookie, err := r.Cookie("svcName")
	if err != nil {
		return ""
	}
	return cookie.Value
}

func createInChannel(reqName string) chan *ClientMessage {
	// XXX: Allow service to define the size of the channel
	reqSize := 50
	inChLock.RLock()
	inChannel := inChMap[reqName]
	inChLock.RUnlock()
	if reqName == "" || reqSize <= 0 {
		// Deny channel request if name is empty or size is negative or zero
		log.Println("WS: Bad channel request! Invalid arguments:", reqName, reqSize)
		return nil
	} else if inChannel != nil {
		// Deny channel request if named channel already exists
		log.Println("WS: Bad channel request! Name already exists:", reqName, reqSize)
		return nil
	} else {
		newChannel := make(chan *ClientMessage, reqSize)
		inChLock.Lock()
		inChMap[reqName] = newChannel
		inChLock.Unlock()
		log.Println("WS: Created input channel:", reqName, reqSize)
		return newChannel
	}
}
