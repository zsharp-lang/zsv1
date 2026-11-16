# When to write a dispatcher

A dispatcher changes how an operation is done for every object. That is the right tool
less often than it looks: most new semantics belong to a CO, and only some to a
dispatcher.

## A CO, when the objects are yours

If the objects whose behavior you want are yours, as the COs of a kind of definition you
defined, implement the operation's capability on them. A class is called because its CO
is `ICTCallable`; nothing else needed to change. The operation's default dispatcher asks
the objects what they do, so a new kind of object takes part with no dispatcher at all.

## A dispatcher, when the objects aren't

A dispatcher is for behavior that belongs to no single object:

- **A relation between objects that neither knows.** Text going where a number is
  expected, in [how REIs work](reis.md#a-dispatcher-of-your-own), is about `String` and
  `Int32` together, and neither is yours. A trait is implemented for a type
  apart from both, so neither the value's CO nor the trait's can say the value goes to
  the trait. A dispatcher that looks the implementation up can, which is what
  `std:traits/static` does.
- **A rule over every object of a kind**, as a conversion every number of one width has
  to a wider one, which no single CO should carry.
- **A different way of deciding**, as `std:traits/dynamic` matches a trait by the type a
  value has as the code runs, not by its static type.

## A new REI, when there is no operation

If what you want isn't an operation that exists, it is an REI of its own. Matching and
overloading are REIs apart from CG, since not every object needs them: a CO uses them when
it wants. An REI is a set of delegates with default entrypoints that give unsupported, and
the operations it has, which code calls and dispatchers replace.

## What a dispatcher may ask

A dispatcher is the one place that may ask what a CO is, with `Is<T>`, because deciding
how an operation applies to an object is its job. Even there it is a restriction: an
object takes part only if it implements what the dispatcher asks for, so custom semantics
need a wrapper to take part. A dispatcher that looks up what it needs outside the
objects, as traits do, restricts nothing.
