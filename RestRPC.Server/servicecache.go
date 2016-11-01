package main

import (
	"sync"

	"./pkg/cachestore"
)

var serviceCache = cachestore.ServiceCache{map[string]*cachestore.KVStore{}, sync.RWMutex{}}
