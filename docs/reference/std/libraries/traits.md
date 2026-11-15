# std:traits

Traits, and their implementations for types, which are definitions of their own:
`def StringGreeting of Implementation[Greeting, String] { ... }`. `std:traits/static` and
`std:traits/dynamic` are what make a value go where its trait is expected: from its
static type, or from the type it has as the code runs.
