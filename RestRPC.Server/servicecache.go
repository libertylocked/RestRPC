package main

import (
	"sync"

	"./pkg/cachestore"
)

var serviceCache = cachestore.CacheStore{map[string]*cachestore.KVStore{}, sync.RWMutex{}}
