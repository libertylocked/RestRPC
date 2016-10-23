package main

import (
	"sync"

	"github.com/satori/go.uuid"
)

// ClientMessage represents an input request sent by web clients
type ClientMessage struct {
	// Set by server. The header of the request
	Header string `json:",omitempty"`
	// The target component this request is going to. This field is omitted when sending to the component
	Target string `json:",omitempty"`
	// The remote procedure to call
	Cmd string
	// Arguments to be passed to the remote procedure
	Args []interface{}
	// Set by server. UID attached to this request is the unique ID of the requester
	// In POST, this is unique per POST. In WS, this is unique per WS connection
	UID uuid.UUID
	// Custom ID. Only used by WS clients, not POST clients
	// This field is not necessarily unique
	CID string `json:",omitempty"`
}

// ServiceMessage is the message sent by RRPC components
type ServiceMessage struct {
	Header string
	Data   interface{}
	UID    string
	CID    string
}

// ProcedureReturn is the message sent to clients
// Data and CID comes from response ServiceMessage
// i.e. a trimmed version of WebOutput containing only the information clients are interested in
type ProcedureReturn struct {
	Data interface{}
	CID  string
}

// GetProcedureReturn gets a procedure return message from a Service Message
func (svcMsg *ServiceMessage) GetProcedureReturn() *ProcedureReturn {
	retMsg := ProcedureReturn{svcMsg.Data, svcMsg.CID}
	return &retMsg
}

var inChMap = map[string]chan *ClientMessage{} // Input channel map, used to send inputs from web to components. Key is component ID
var inChLock sync.RWMutex
var retChMap = map[uuid.UUID]chan *ProcedureReturn{} // Return data map, used for RRPC component send return data to its requester
var retChLock sync.RWMutex
