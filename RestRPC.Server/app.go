package main

import (
	"log"
	"net/http"

	"github.com/goji/httpauth"
	"github.com/gorilla/mux"
)

func main() {
	readConfig()
	readPasswords()

	http.Handle("/", httpauth.BasicAuth(userAuthOpts)(getHandlers()))
	http.Handle("/componentws", httpauth.BasicAuth(componentAuthOpts)(http.HandlerFunc(handleComponentWS)))

	http.Handle("/static/", http.StripPrefix("/static/", http.FileServer(http.Dir("static"))))
	http.Handle("/apps/", http.StripPrefix("/apps/", http.FileServer(http.Dir("apps"))))

	log.Println("Listening on port " + serverConfig.Server.Port)
	if serverConfig.TLS.UseHTTPS {
		log.Fatal(http.ListenAndServeTLS(":"+serverConfig.Server.Port, serverConfig.TLS.Cert, serverConfig.TLS.Key, nil))
	} else {
		log.Fatal(http.ListenAndServe(":"+serverConfig.Server.Port, nil))
	}
}

func getHandlers() *mux.Router {
	router := mux.NewRouter()
	router.HandleFunc("/status", handleStatusGet).Methods("GET")
	router.HandleFunc("/cache", handleCacheGet).Methods("GET")
	router.HandleFunc("/input", handleInputPost).Methods("POST")
	router.HandleFunc("/clientws", handleClientWS)
	router.HandleFunc("/", handleIndex).Methods("GET")
	return router
}
