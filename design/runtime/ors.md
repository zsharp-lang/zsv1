# Runtime ORS

The runtime system defines its own ORS domain entities. These
entities are things like `Class`, `Interface`, `Delegate`, etc..

Basically, for each CTS entity, we define an ORS entity. To be
more precise, we define *more* entities than the CTS itself.
Since ORS is properly structured, we can't have a single `Class`
entity representing both a normal class and a generic class. The
reason for that is since a class is indeed valid type, but a
generic class is not! So these are 2 separate entities, with
different markers.
