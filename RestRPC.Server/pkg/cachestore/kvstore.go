package cachestore

import "sync"

// KVStore represents a KV store for a service
// Its Get() and Set() guarantee thread safety
type KVStore struct {
	// The KV store. Value is expected to be JSON serialized
	Map map[string]string
	// The lock for this particular KV store
	Mutex sync.RWMutex `json:"-"`
}

// Set sets a KV pair in the store
func (store *KVStore) Set(key string, value string) {
	store.Mutex.Lock()
	store.Map[key] = value
	store.Mutex.Unlock()
}

// Get gets the value associated with the key
func (store *KVStore) Get(key string) string {
	store.Mutex.RLock()
	val := store.Map[key]
	store.Mutex.RUnlock()
	return val
}
