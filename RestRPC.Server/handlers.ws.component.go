package main

import (
	"encoding/json"
	"log"
	"net/http"
	"reflect"

	"github.com/gorilla/websocket"
	"github.com/satori/go.uuid"
)

// WebOutput is the output message sent by WSH components
type WebOutput struct {
	Header string
	Data   interface{}
	UID    string
	CID    string
}

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
		log.Println("Component disconnected:", r.RemoteAddr)
	}()
	log.Println("Component connected:", r.RemoteAddr)

	for {
		// Read message from plugin
		// This will block until an output (return value, pulse, etc) is received.
		_, data, err := c.ReadMessage()
		if err != nil {
			log.Println("WS:", err)
			break
		}
		var output WebOutput
		json.Unmarshal(data, &output)
		processOutput(output, c)
	}
}

func processOutput(output WebOutput, c *websocket.Conn) {
	defer func() {
		if rec := recover(); rec != nil {
			log.Println("Error processing component message:", output, rec)
			return
		}
	}()

	// Read the header
	switch output.Header {
	case "n":
		req := reflect.ValueOf(output.Data)
		// A channel open request
		// An unsuccessful channel request will cause the server to close the connection
		reqName := req.Index(0).Interface().(string)
		reqSize := int(req.Index(1).Interface().(float64))
		if reqName == "" || reqSize <= 0 {
			// Deny channel request if name is empty or size is negative or zero
			log.Println("WS: Bad channel request! Invalid arguments:", output.Data)
			// XXX: Send a response to service instead of closing connection
			c.Close()
		} else if inChMap[reqName] != nil || registeredConnections[c] != "" {
			// Deny channel request if named channel already exists, or
			// the connection has already registered a name
			log.Println("WS: Bad channel request! Name already exists:", output.Data)
			// XXX: Send a response to service instead of closing connection
			c.Close()
		} else {
			registeredConnections[c] = reqName
			inChMap[reqName] = make(chan WebInput, reqSize)
			log.Println("WS: Component requested input channel:", output.Data)
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
		uid, _ := uuid.FromString(output.UID)
		if retChMap[uid] != nil {
			retChMap[uid] <- output.Data
			log.Println("WS: Returned output for:", uid)
		}
	default:
		log.Println("WS: Unknown output format:", output)
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
