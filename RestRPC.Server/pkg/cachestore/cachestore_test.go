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

import "testing"

func TestCreateStore(t *testing.T) {
	cache := NewCacheStore()
	store := cache.CreateStore("foo")

	if store == nil {
		t.Error("KVStore returned is nil")
	}
	if cache.GetStore("foo") == nil {
		t.Error("KVStore is nil")
	}
}

func TestGetStore(t *testing.T) {
	cache := NewCacheStore()
	var services = []string{"foo", "bar", "svc", "biz"}

	for _, s := range services {
		newStore := cache.CreateStore(s)
		gotStore := cache.GetStore(s)
		if newStore != gotStore {
			t.Error("Got a different store than the one created for:", s)
		}
	}
}

func TestDeleteStore(t *testing.T) {
	cache := NewCacheStore()
	var services = []string{"foo", "bar", "svc", "biz"}

	for _, s := range services {
		cache.CreateStore(s)
		cache.DeleteStore(s)
		if cache.GetStore(s) != nil {
			t.Error("Store is expected to have been deleted, but has not", s)
		}
	}
}

func TestSetGetCache(t *testing.T) {
	cache := NewCacheStore()

	// Assert the store isn't created
	if cache.GetStore("foo") != nil {
		t.Error("Store foo is not nil in a new CacheStore")
	}

	cache.SetCache("foo", "baz", "snake")

	// Assert the store is created by SetCache
	if cache.GetStore("foo") == nil {
		t.Error("Store foo not created by SetCache")
	}

	if got := cache.GetCache("foo", "baz"); got != "snake" {
		t.Error("Did not get the same value as the one set:", got)
	}
}
