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
	newChan := createReqChannel(svcName)
	if newChan == nil {
		// XXX: Close connection if channel creation failed. Should respond with an error message
		c.Close()
	} else {
		// If channel is successfully created, start input delivery routine
		go deliverRequestRoutine(newChan, c)
		defer func() {
			// Close the channel and remove from in channel map
			close(newChan)
			reqChLock.Lock()
			delete(reqChMap, svcName)
			reqChLock.Unlock()
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
		//var msg OutMessage
		var msgObjMap map[string]*json.RawMessage
		err = json.Unmarshal(data, &msgObjMap)
		if err != nil {
			log.Println("WS: Error unmarshalling message!", err, string(data))
		} else {
			processServiceMessage(msgObjMap, c, r)
		}
	}
}

func processServiceMessage(msgObjMap map[string]*json.RawMessage, c *websocket.Conn, r *http.Request) {
	defer func() {
		if rec := recover(); rec != nil {
			log.Println("WS: Error processing service message:", msgObjMap, rec)
			return
		}
	}()

	// Unmarshal the header
	var header string
	err := json.Unmarshal(*msgObjMap["Header"], &header)
	if err != nil {
		panic(err)
	}

	// Read the header
	switch header {
	case HeaderCacheRequest:
		// A cache request
		// TODO: not implemented

	case HeaderResponse:
		// A response object
		var responseObject ResponseObject
		err = json.Unmarshal(*msgObjMap["Data"], &responseObject)
		if err != nil {
			panic(err)
		}

		// Clear RID from the response object before enqueuing
		rid, _ := uuid.FromString(responseObject.RID)
		responseObject.RID = ""

		retChLock.RLock()
		retChannel := retChMap[rid]
		retChLock.RUnlock()
		select {
		case retChannel <- &responseObject:
			log.Println("WS: Returned output for:", rid)
		default:
			// Fails if either the return channel is full, or the return channel has been deleted
			log.Println("WS: Return channel unavailable. Discarding:", msgObjMap)
		}

	default:
		log.Println("WS: Unknown service message format:", msgObjMap)
	}
}

// A routine to continue deliver request messages to service until channel is closed
func deliverRequestRoutine(reqChannel chan *RequestObject, c *websocket.Conn) {
	for {
		requestObject, ok := <-reqChannel
		if ok {
			// Wrap the request object in an InMessage
			inMsg := InMessage{"", requestObject}
			errWrite := c.WriteJSON(inMsg)
			if errWrite != nil {
				log.Println("WS:", errWrite)
			}
			log.Println("WS: Dequeued:", requestObject.RID)
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

func createReqChannel(svcName string) chan *RequestObject {
	// XXX: Allow service to define the size of the channel
	qSize := 50
	reqChLock.RLock()
	reqChannel := reqChMap[svcName]
	reqChLock.RUnlock()
	if svcName == "" || qSize <= 0 {
		// Deny channel request if name is empty or size is negative or zero
		log.Println("WS: Bad channel request! Invalid arguments:", svcName, qSize)
		return nil
	} else if reqChannel != nil {
		// Deny channel request if named channel already exists
		log.Println("WS: Bad channel request! Name already exists:", svcName, qSize)
		return nil
	} else {
		newChannel := make(chan *RequestObject, qSize)
		reqChLock.Lock()
		reqChMap[svcName] = newChannel
		reqChLock.Unlock()
		log.Println("WS: Created input channel:", svcName, qSize)
		return newChannel
	}
}
