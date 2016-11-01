package main

// headers set by service in messages sent to server
const (
	HeaderResponse     = ""
	HeaderCacheRequest = "c"
)

type Message struct {
	Header string
	Data   interface{}
}
