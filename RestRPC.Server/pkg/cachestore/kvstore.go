/*
Copyright (c) 2016 libertylocked

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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

// NewKVStore creates a new KVStore
func NewKVStore() *KVStore {
	return &KVStore{map[string]string{}, sync.RWMutex{}}
}
