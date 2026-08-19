# ORS vs OIS

The ORS system defines a small set of marker interface, used for
typing slots in a graph. However, ORS entities should strictly
never define polymorphic behavior. That is, an ORS entity is only
a data structure rather than an actual object.

The OIS system, however, defines a single entity type, `CompilerObject`.
This type is an opaque handler which should never, EVER, be typed
matched against. The only semi-exception to this rule are the
interation APIs implementation. These may use the `.Is<T>` method
to check whether a certain object implements some capabiltity,
since at some point we're going to have to know what we're working
with. However, keep in mind that the interaction APIs implementation
worry about more fine-grained typing so that we, the consumers of
interaction APIs, don't need to, and we can simply pass in a CO,
hoping for the best (since every interaction either produces a CO
or an Error).

So, ORS is meant to be used for reflection and programmatic code
generation, while the OIS is meant to be used for blind object
interaction.

Generally, the flow is to go from ORS -> OIS.
That is, we define a strucutre, and some sort of OIS importer
creates a CO that implements the semantics of the given ORS object.
