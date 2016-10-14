package main

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/gorilla/websocket"
	"github.com/satori/go.uuid"
)

// WebOutput is the output message sent by WSH components
type WebOutput struct {
	Header string
	Data   []interface{}
	UID    string
}

var upgrader = websocket.Upgrader{
	ReadBufferSize:  1024,
	WriteBufferSize: 1024,
}

var componentNameMap = make(map[*websocket.Conn]string) // Maps a component WS connection to its name

func handleComponentWS(w http.ResponseWriter, r *http.Request) {
	c, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("WS:", err)
		return
	}
	defer func() {
		// Remove the component name from name map and its channel from channel map
		c.Close()
		delete(inChMap, componentNameMap[c])
		delete(componentNameMap, c)
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
	// defer func() {
	// 	if rec := recover(); rec != nil {
	// 		fmt.Println("Error processing component output:", output)
	// 		return
	// 	}
	// }()

	// Read the header
	switch output.Header {
	case "n":
		// A channel open request
		componentNameMap[c] = output.Data[0].(string)
		inChMap[componentNameMap[c]] = make(chan WebInput, int(output.Data[1].(float64)))
		log.Println("WS: Component requested input channel:", output.Data)
	case "p", "":
		// A pulse can have either 'p' header, or no header at all
		// Dequeue all inputs and send to component, if a channel has been made
		if componentNameMap[c] != "" && inChMap[componentNameMap[c]] != nil {
			deliverInputs(c, componentNameMap[c])
		}
	case "c":
		// A cache request

	case "r":
		// A return value
		uid, _ := uuid.FromString(output.UID)
		if retChMap[uid] != nil && len(output.Data) != 0 {
			retChMap[uid] <- output.Data[0]
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
