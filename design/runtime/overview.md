# Overview

The runtime itself is very minimal. The reason for that being that Z# runs directly
on top of .NET and uses the CLR as its own runtime.

The runtime is implemeted as a platform compiler. That is, it takes some LIR graph
and does things.

As platform compilers naturally do, the runtime also defines some "primitive" object
and pre-maps some pre-defined LIR nodes to them

The important thing to keep in mind is that the runtime is *just another* platform
compiler that happens to load LIR into the .NET runtime (via `System.Reflection.Emit`).
