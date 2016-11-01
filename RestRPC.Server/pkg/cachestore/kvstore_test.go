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

import (
	"math/rand"
	"strconv"
	"sync"
	"testing"
	"time"
)

func TestSetAndGetValues(t *testing.T) {
	store := NewKVStore()

	kvpairs := []struct {
		key, value string
	}{
		{"Hello", "世界"},
		{"vivify", "otherbar"},
		{"foo", ""},
		{"", "baz"},
	}

	for _, pair := range kvpairs {
		store.Set(pair.key, pair.value)
		got := store.Get(pair.key)
		if got != pair.value {
			t.Errorf("Got %s for key %s, expected: %s", got, pair.key, pair.value)
		}
	}
}

func TestConcurrentRW(t *testing.T) {
	rand.Seed(time.Now().UnixNano())
	const letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
	var letterBytes = []byte(letters)

	const numsets = 5000
	const valuelen = 20

	var wgWrite, wgRead sync.WaitGroup
	wgWrite.Add(numsets)
	wgRead.Add(numsets)

	store := NewKVStore()

	// Generate key value pairs with random values
	kvpairs := [numsets]struct {
		key, value string
	}{}
	for i := 0; i < numsets; i++ {
		b := make([]byte, 20)
		for i := range b {
			b[i] = letterBytes[rand.Int63()%int64(len(letterBytes))]
		}

		kvpairs[i].key = strconv.Itoa(i)
		kvpairs[i].value = string(b)
	}

	startWrite := make(chan struct{})
	for _, pair := range kvpairs {
		// Store the kvp in a goroutine
		go func(key string, value string) {
			defer wgWrite.Done()
			<-startWrite
			store.Set(key, value)
		}(pair.key, pair.value)
	}

	close(startWrite)
	wgWrite.Wait()

	startRead := make(chan struct{})
	for _, pair := range kvpairs {
		// Read the kvp in a goroutine
		go func(key string, value string) {
			defer wgRead.Done()
			<-startRead
			got := store.Get(key)
			if got != value {
				t.Errorf("Got %s for key %s, expected: %s", got, key, value)
			}
		}(pair.key, pair.value)
	}

	close(startRead)
	wgRead.Wait()
}
