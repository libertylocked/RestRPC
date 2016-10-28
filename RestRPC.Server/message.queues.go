package main

import (
	"sync"

	"github.com/satori/go.uuid"
)

// Input channel map, used to send messages to components. Key is component ID
var reqChMap = map[string]chan *RequestObject{}
var reqChLock sync.RWMutex

// Return data map, used for RRPC component return data to its requester. Key is requster ID
var retChMap = map[uuid.UUID]chan *ResponseObject{}
var retChLock sync.RWMutex
