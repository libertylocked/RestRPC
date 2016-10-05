package main

import (
	"log"
	"net/http"

	"github.com/gorilla/mux"
)

func main() {
	readConfig()

	http.Handle("/", getHandlers())
	http.Handle("/static/", http.StripPrefix("/static/", http.FileServer(http.Dir("static"))))
	http.Handle("/apps/", http.StripPrefix("/apps/", http.FileServer(http.Dir("apps"))))
	log.Println("Listening on port " + serverConfig.Server.Port)
	if serverConfig.TLS.UseHTTPS {
		http.ListenAndServeTLS(":"+serverConfig.Server.Port, serverConfig.TLS.Cert, serverConfig.TLS.Key, nil)
	} else {
		http.ListenAndServe(":"+serverConfig.Server.Port, nil)
	}
}

func getHandlers() *mux.Router {
	router := mux.NewRouter()
	router.HandleFunc("/componentws", handleComponentWS)
	router.HandleFunc("/status", handleStatusGet).Methods("GET")
	router.HandleFunc("/input", handleInputPost).Methods("POST")
	router.HandleFunc("/fetch", handleFetchGet).Methods("GET")
	router.HandleFunc("/", handleIndex).Methods("GET")
	return router
}
