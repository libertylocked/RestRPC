package main

import "encoding/json"

type RequestObject struct {
	Method string
	Params []interface{}
	ID     string
	// This field is omitted when sending to RPC service
	TID string `json:",omitempty"`
	// This field is set by server
	RID string
}

type ResponseObject struct {
	Result *json.RawMessage
	Error  *ErrorObject `json:",omitempty"`
	ID     string
	// This field is omitted when sending to RPC caller
	RID string `json:",omitempty"`
}

type ErrorObject struct {
	Code    int
	Message string
	Data    string
}

type CacheObject struct {
	Key   string
	Value *json.RawMessage
}
