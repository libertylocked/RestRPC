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

func handleComponentWS(w http.ResponseWriter, r *http.Request) {
	c, err := upgrader.Upgrade(w, r, nil)
	newCookie := http.Cookie{}
	r.AddCookie(&newCookie)
	if err != nil {
		log.Println("WS:", err)
		return
	}

	defer func() {
		// Remove the component name from name map and its channel from channel map
		c.Close()
		inChLock.Lock()
		inChannel := inChMap[getComponentNameCookie(r)]
		if inChannel != nil {
			// Close the channel
			close(inChannel)
			delete(inChMap, getComponentNameCookie(r))
		}
		inChLock.Unlock()
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
	case "n":
		req := reflect.ValueOf(msg.Data)
		// A channel open request
		// An unsuccessful channel request will cause the server to close the connection
		reqName := req.Index(0).Interface().(string)
		reqSize := int(req.Index(1).Interface().(float64))
		inChLock.RLock()
		inChannel := inChMap[reqName]
		inChLock.RUnlock()
		if reqName == "" || reqSize <= 0 {
			// Deny channel request if name is empty or size is negative or zero
			log.Println("WS: Bad channel request! Invalid arguments:", msg.Data)
			// XXX: Send a response to service instead of closing connection
			c.Close()
		} else if inChannel != nil || getComponentNameCookie(r) != "" {
			// Deny channel request if named channel already exists, or
			// the connection has already registered a name
			log.Println("WS: Bad channel request! Name already exists:", msg.Data)
			// XXX: Send a response to service instead of closing connection
			c.Close()
		} else {
			// Set component name, and make a new channel for this component
			setComponentNameCookie(reqName, r)
			newChannel := make(chan *ClientMessage, reqSize)
			inChLock.Lock()
			inChMap[reqName] = newChannel
			inChLock.Unlock()
			// Start a goroutine to listen on this channel and deliver inputs, until channel closes
			go deliverInputsRoutine(newChannel, c)
			log.Println("WS: Component requested input channel:", msg.Data)
		}

	case "c":
		// A cache request

	case "r":
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

func setComponentNameCookie(name string, r *http.Request) {
	cookie, err := r.Cookie("svcName")
	if err != nil {
		// Svc name cookie does not exist
		ck := http.Cookie{Name: "svcName", Value: name}
		r.AddCookie(&ck)
	} else {
		// Cookie does exist: overwrite it
		cookie.Value = name
	}
}

func getComponentNameCookie(r *http.Request) string {
	cookie, err := r.Cookie("svcName")
	if err != nil {
		return ""
	}
	return cookie.Value
}
