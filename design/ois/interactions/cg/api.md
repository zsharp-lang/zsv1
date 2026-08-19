# CG API - Code-Generation API


The `CG` API defines interactions that are used during compilation of code.
If we look at the operations expresion code can do we see a small list:
* Assign
* Call
* Cast
* Index
* Member

We also have unary/binary operators, but these we can pre-translate to `Call`
at a previous stage.

There is a less apparent yet no-less important operation we haven't listed.
I'm referring to the `Get` operation. We'll add it to the list.

The `Index` operation could've also been translated to a `Call`. However, it
needs to be different so the target object can do different things for a call
and an index access, since both have the same interaction shape.

To make thing a bit more uniform we can redefine the list as this:
* Call
* Cast
* Get
* Index
* Member
* Set


## Interaction Shape

I'll use a custom syntax. I'll also use `CO` instead of `CompilerObject`. All operations return an `InteractionResult`.
* `Call(object: CO, arguments: Array<Argument>)`
* `Cast(object: CO, type: CO)`
* `Get(object: CO)`
* `Index(object: CO, arguments: Array<Argument>)`
* `Member(object: CO, name: String)`
* `Member(object: CO, index: Integer)`
* `Set(object: CO, value: CO)`

The `Argument` object is a pair of an optional name and a CO, and thus is trivially convertible from CO by construction.

There are 2 `Member` operations. Both are related to the `.` syntax. That is, the member specifier is compile-time.
> NOTE: I have referred to syntax above. The CG API has *nothing* to do with syntax. I only referred to it to explain
> why there are 2 variants. The key takeaway is to realize `Index` takes in a number of COs, while `Member` takes a 
> compile-time value. 
