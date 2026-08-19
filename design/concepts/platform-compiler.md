# Concept - Platform Compiler

A platform compiler is something that compiles LIR down to a platform
specific representation. Due to the very ambigous term of "platform
specific representation", this is defined as a *concept* rather than
an actual API.

The general way to go about platform compilers is to implement them
as 2 parts:
1) Cache<LIR -> PSR> (platform-specific-representaiton)
2) Compile<LIR -> PSR>

The compilation process handles all LIR graphs. It caches the result
object when it's done.
Platform primitives are then simply implemented as pre-cached PSR
objects, mapped from pre-created LIR objects that act as markers.
Then, all is left is to have these LIR markers somewhere accessible
and viola! we can compile any LIR graph to that platform, with the
ability to reference primitives.
