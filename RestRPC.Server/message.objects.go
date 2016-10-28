package main

type RequestObject struct {
	Method string
	Params []interface{}
	ID     string
	// This field is omitted when sending to RPC service. Omit before enqueuing
	TID string `json:",omitempty"`
	// This field is set by server
	RID string
}

type ResponseObject struct {
	Result interface{}
	Error  ErrorObject `json:",omitempty"`
	ID     string
	// This field is omitted when sending to RPC caller. Omit before enqueuing
	RID string `json:",omitempty"`
}

type ErrorObject struct {
	Code    int    `json:",omitempty"`
	Message string `json:",omitempty"`
	Data    string `json:",omitempty"`
}
