package main

import (
	"encoding/json"
	"io/ioutil"
	"log"
	"net/http"

	"github.com/goji/httpauth"
)

var componentAuthOpts = httpauth.AuthOptions{
	Realm:    "Component",
	AuthFunc: componentAuthFunc,
}

var userAuthOpts = httpauth.AuthOptions{
	Realm:    "User",
	AuthFunc: userAuthFunc,
}

var componentPasswords = map[string]string{}
var userPasswords = map[string]string{}

func readPasswords() {
	data, _ := ioutil.ReadFile("auth.component.json")
	err := json.Unmarshal(data, &componentPasswords)
	if err != nil {
		log.Printf("Error reading component passwords: %s", err)
	}
	data, _ = ioutil.ReadFile("auth.user.json")
	err = json.Unmarshal(data, &userPasswords)
	if err != nil {
		log.Printf("Error reading user passwords: %s", err)
	}
}

func componentAuthFunc(username, password string, r *http.Request) bool {
	if password != "" && password == componentPasswords[username] {
		log.Printf("Component authenticated: %s", username)
		return true
	}

	return false
}

func userAuthFunc(username, password string, r *http.Request) bool {
	if password != "" && password == userPasswords[username] {
		return true
	}

	return false
}
