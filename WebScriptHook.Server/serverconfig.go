package main

import (
	"fmt"
	"io/ioutil"
	"log"

	"gopkg.in/gcfg.v1"
)

var serverConfig = struct {
	Server struct {
		Port string
	}
	Message struct {
		Timeout int
	}
	TLS struct {
		UseHTTPS bool
		Cert     string
		Key      string
	}
	WebSocketAuth struct {
		Username string
		Password string
	}
}{}

func readConfig() {
	log.Println("Reading config")
	filebytes, _ := ioutil.ReadFile("webscripthook.server.ini")
	cfgStr := string(filebytes)
	fmt.Println(cfgStr)
	err := gcfg.ReadStringInto(&serverConfig, cfgStr)
	if err != nil {
		log.Fatalf("Failed to parse gcfg data: %s", err)
	}
}
