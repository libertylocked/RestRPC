package main

import (
	"encoding/json"
	"log"
	"net/http"
	"reflect"

	"github.com/gorilla/websocket"
	"github.com/satori/go.uuid"
)

var upgrader = websocket.Upgrader{
	ReadBufferSize:  1024,
	WriteBufferSize: 1024,
}

var registeredConnections = map[*websocket.Conn]string{} // Maps a component WS connection to its name

func handleComponentWS(w http.ResponseWriter, r *http.Request) {
	c, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("WS:", err)
		return
	}
	defer func() {
		// Remove the component name from name map and its channel from channel map
		c.Close()
		delete(inChMap, registeredConnections[c])
		delete(registeredConnections, c)
		log.Println("WS: Component disconnected:", r.RemoteAddr)
	}()
	log.Println("WS: Component connected:", r.RemoteAddr)

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
			processServiceMessage(msg, c)
		}
	}
}

func processServiceMessage(msg ServiceMessage, c *websocket.Conn) {
	defer func() {
		if rec := recover(); rec != nil {
			log.Println("WS: Error processing service message:", msg, rec)
			return
		}
	}()

	// Read the header
	switch msg.Header {
	case "n":
		req := reflect.ValueOf(msg.Data)
		// A channel open request
		// An unsuccessful channel request will cause the server to close the connection
		reqName := req.Index(0).Interface().(string)
		reqSize := int(req.Index(1).Interface().(float64))
		if reqName == "" || reqSize <= 0 {
			// Deny channel request if name is empty or size is negative or zero
			log.Println("WS: Bad channel request! Invalid arguments:", msg.Data)
			// XXX: Send a response to service instead of closing connection
			c.Close()
		} else if inChMap[reqName] != nil || registeredConnections[c] != "" {
			// Deny channel request if named channel already exists, or
			// the connection has already registered a name
			log.Println("WS: Bad channel request! Name already exists:", msg.Data)
			// XXX: Send a response to service instead of closing connection
			c.Close()
		} else {
			registeredConnections[c] = reqName
			inChMap[reqName] = make(chan ClientMessage, reqSize)
			log.Println("WS: Component requested input channel:", msg.Data)
		}

	case "p", "":
		// A pulse can have either 'p' header, or no header at all
		// Dequeue all inputs and send to component, if a channel has been made
		if registeredConnections[c] != "" && inChMap[registeredConnections[c]] != nil {
			deliverInputs(c, registeredConnections[c])
		}

	case "c":
		// A cache request

	case "r":
		// A return value
		uid, _ := uuid.FromString(msg.UID)
		retMsg := ProcedureReturn{msg.Data, msg.CID}
		select {
		case retChMap[uid] <- retMsg:
			log.Println("WS: Returned output for:", uid)
		default:
			// Fails if either the return channel is full, or the return channel has been deleted
			log.Println("WS: Return channel unavailable. Discarding:", msg)
		}

	default:
		log.Println("WS: Unknown service message format:", msg)
	}
}

func deliverInputs(c *websocket.Conn, componentName string) {
	inputQueueEmpty := false
	for !inputQueueEmpty {
		select {
		case input, ok := <-inChMap[componentName]:
			if ok {
				errWrite := c.WriteJSON(input)
				if errWrite != nil {
					log.Println("WS:", errWrite)
				}
				log.Println("WS: Dequeued:", input.UID)
			} else {
				log.Println("WS: Channel closed!")
			}
		default:
			// The input message queue for this component is empty now
			inputQueueEmpty = true
		}
	}
}
