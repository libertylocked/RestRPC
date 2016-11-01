package main

import (
	"encoding/json"
	"io"
	"io/ioutil"
	"log"
	"net/http"
	"time"

	"github.com/satori/go.uuid"
)

func handleStatusGet(w http.ResponseWriter, r *http.Request) {
	// Lists all the components connected and their remote endpoints
	componentList := []string{}
	reqChLock.RLock()
	for k := range reqChMap {
		componentList = append(componentList, k)
	}
	reqChLock.RUnlock()
	jsonOutput, err := json.Marshal(componentList)
	if err != nil {
		http.Error(w, http.StatusText(500), 500)
	}
	w.Write(jsonOutput)
}

func handleCacheGet(w http.ResponseWriter, r *http.Request) {
	svc := r.URL.Query().Get("svc")
	key := r.URL.Query().Get("key")
	if svc != "" && key != "" {
		io.WriteString(w, serviceCache.GetCache(svc, key).(string))
		return
	}

	// If svc or key is not specified, return the entire cache store as a JSON object
	// This will RLock all the stores, until stores are serialized and written to requester!
	cacheMarshalled, err := json.Marshal(serviceCache)
	if err != nil {
		log.Println("Error marshalling cache", err)
	}
	w.Write(cacheMarshalled)
}

func handleInputPost(w http.ResponseWriter, r *http.Request) {
	// REST client sends input to game via POST
	decoder := json.NewDecoder(r.Body)
	var reqObject RequestObject
	err := decoder.Decode(&reqObject)
	if err != nil {
		log.Println("POST: Error decoding input:", err)
		return
	}
	requesterID := uuid.NewV4()
	// Attach requester ID
	reqObject.RID = requesterID.String()
	// Clear the TID before enqueuing
	targetID := reqObject.TID
	reqObject.TID = ""

	reqChLock.RLock()
	reqChannel := reqChMap[targetID]
	reqChLock.RUnlock()

	select {
	case reqChannel <- &reqObject:
		log.Println("POST: Sent:", reqObject)
		// If ID is null or empty, it's a notification - don't need to return anything
		if reqObject.ID == "" {
			w.WriteHeader(http.StatusAccepted)
			return
		}
		// If ID is not empty, create a return channel and wait for response
		retChannel := make(chan *ResponseObject, 1)
		retChLock.Lock()
		retChMap[requesterID] = retChannel
		retChLock.Unlock()
		defer func() {
			close(retChannel)
			retChLock.Lock()
			delete(retChMap, requesterID)
			retChLock.Unlock()
		}()
		timeout := make(chan bool, 1)
		go func() {
			time.Sleep(time.Duration(serverConfig.Message.Timeout) * time.Millisecond)
			timeout <- true
		}()
		// Now we wait till the component sends the return value back, or it times out
		select {
		case responseObject := <-retChannel:
			// A return message has arrived!
			seralizedRet, err := json.Marshal(responseObject)
			if err != nil {
				log.Println("POST: Error marshalling return data:", err)
				http.Error(w, http.StatusText(http.StatusBadGateway), http.StatusBadGateway)
				return
			}
			// Return value successfully retrieved
			log.Println("POST: Returned:", string(seralizedRet))
			w.Header().Set("Content-Type", "application/json")
			w.Write(seralizedRet)
			return
		case <-timeout:
			// Return value did not arrive in time
			log.Println("POST: Return timeout:", requesterID)
			http.Error(w, http.StatusText(http.StatusGatewayTimeout), http.StatusGatewayTimeout)
			return
		}
	default:
		// Fails to POST the input because component's chan is full, or its channel does not exist
		log.Println("POST: Input channel unavailable. Discarding:", reqObject)
		http.Error(w, http.StatusText(http.StatusBadGateway), http.StatusBadGateway)
		return
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
