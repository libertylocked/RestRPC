package main

import (
	"encoding/json"
	"io/ioutil"
	"log"
	"net/http"
	"time"

	"github.com/satori/go.uuid"
)

func handleStatusGet(w http.ResponseWriter, r *http.Request) {
	// Lists all the components connected and their remote endpoints
	componentList := []string{}
	inChLock.RLock()
	for k := range inChMap {
		componentList = append(componentList, k)
	}
	inChLock.RUnlock()
	jsonOutput, err := json.Marshal(componentList)
	if err != nil {
		http.Error(w, http.StatusText(500), 500)
	}
	w.Write(jsonOutput)
}

func handleInputPost(w http.ResponseWriter, r *http.Request) {
	// REST client sends input to game via POST
	decoder := json.NewDecoder(r.Body)
	var input ClientMessage
	err := decoder.Decode(&input)
	if err != nil {
		log.Println("POST: Error decoding input:", err)
		return
	}
	// Attach a UUID to the request input
	input.UID = uuid.NewV4()
	// Get the target of the request, then clear it from the input so we don't send it to component
	targetID := input.Target
	input.Target = ""

	inChLock.RLock()
	inChannel := inChMap[targetID]
	inChLock.RUnlock()

	select {
	case inChannel <- &input:
		log.Println("POST: Sent:", input)
		// Make a channel and wait for the return value for this input
		retChannel := make(chan *ProcedureReturn, 1)
		retChLock.Lock()
		retChMap[input.UID] = retChannel
		retChLock.Unlock()
		defer func() {
			retChLock.Lock()
			delete(retChMap, input.UID)
			retChLock.Unlock()
		}()
		timeout := make(chan bool, 1)
		go func() {
			time.Sleep(time.Duration(serverConfig.Message.Timeout) * time.Millisecond)
			timeout <- true
		}()
		// Now we wait till the component sends the return value back, or it times out
		select {
		case retMessage := <-retChannel:
			// A return message has arrived!
			// Only need the data portion for POST. CID is only used for requesters on WS
			seralizedRet, err := json.Marshal(retMessage.Data)
			if err != nil {
				log.Println("POST: Error marshalling return data:", err)
			} else {
				// Return value successfully retrieved
				log.Println("POST: Returned:", string(seralizedRet))
				w.Write(seralizedRet)
			}
		case <-timeout:
			// Return value did not arrive in time
			log.Println("POST: Return timeout:", input.UID)
			http.Error(w, http.StatusText(504), 504)
		}
	default:
		// Fails to POST the input because component's chan is full, or its channel does not exist
		log.Println("POST: Input channel unavailable. Discarding:", input)
		http.Error(w, http.StatusText(502), 502)
	}
}

func handleIndex(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Cache-Control", "no-cache, no-store, must-revalidate") // HTTP 1.1.
	w.Header().Set("Pragma", "no-cache")                                   // HTTP 1.0.
	w.Header().Set("Expires", "0")                                         // Proxies
	files, _ := ioutil.ReadDir("./apps/")
	tmplData := map[string]interface{}{}
	folders := []string{}
	for _, f := range files {
		if f.IsDir() {
			folders = append(folders, f.Name())
		}
	}
	tmplData["folders"] = folders
	renderTemplate(w, "index.html", tmplData)
}
