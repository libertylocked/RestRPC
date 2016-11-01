package cachestore

import "sync"

// ServiceCache represents a collection of KV stores, one for each service
type ServiceCache struct {
	Stores map[string]*KVStore
	Mutex  sync.RWMutex `json:"-"`
}

// GetStore gets the KV cache store for the service
// Returns nil if store does not exist
func (c *ServiceCache) GetStore(svcName string) *KVStore {
	c.Mutex.RLock()
	kvstore := c.Stores[svcName]
	c.Mutex.RUnlock()

	return kvstore
}

// CreateStore creates a KV store for a service. Overwrites the old store if exists
// Returns the created store
func (c *ServiceCache) CreateStore(svcName string) *KVStore {
	kvstore := &KVStore{map[string]string{}, sync.RWMutex{}}
	c.Mutex.Lock()
	c.Stores[svcName] = kvstore
	c.Mutex.Unlock()

	return kvstore
}

// DeleteStore deletes the KV store for a service. Does nothing if store does not exist
func (c *ServiceCache) DeleteStore(svcName string) {
	c.Mutex.Lock()
	delete(c.Stores, svcName)
	c.Mutex.Unlock()
}

// SetCache sets a KV pair in a service's KV store. Creates the store if not exists
func (c *ServiceCache) SetCache(svcName string, key string, value string) {
	svcStore := c.GetStore(svcName)
	if svcStore == nil {
		svcStore = c.CreateStore(svcName)
	}

	svcStore.Set(key, value)
}

// GetCache gets the value for a given key in a service's KV store.
// Returns "" if store not found, otherwise the value specified by key
func (c *ServiceCache) GetCache(svcName string, key string) string {
	svcStore := c.GetStore(svcName)
	if svcStore == nil {
		return ""
	}
	return svcStore.Get(key)
}
