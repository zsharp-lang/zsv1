# Concept - OIS Importer

Suppose that we want to be able to reference Python code from
Z#. This task seems like it would require a special compiler
import directive of some sort. Well, it doesn't!

Recall that all object interations are done by the OIS, which
defines a very simple form of object: `CompilerObject`.
Since CO is so simple, it's very, very easy to create new types
of COs to completely customize the code-generation process.
To see how this is done, see the CG API and its implementations.

For our example, we'll need something that can read Python files
and create COs from them. These COs should implement Python semantics.

Now, if we also had a platform compiler that compiles to Python, it
would have also been nice if, when compiling to Python, the imported
Python entities we used would also be compiled.

The specifics of how this is done is left to the platform compiler
itself, since that's its job (to generate Python files from LIR).
However, since we're the ones who created the COs, it is our job
to make sure out COs can generate LIR objects that the Python platform
compiler knows how to resolve to an existing Python source file.

That is EXACTLY the reason the LIR API defines the endpoint to also
receive an opaque `platform` object. Using simple type matching, we 
can validate that the given platform is an `IPythonRuntime` and use
the defined API to pre-map our own LIR object to an existing Python
source file. That also means our COs will need to know the source file
and import names they're referencing. However, this should be handled
when we actually construct the COs.

> NOTE: `IPythonRuntime` is not a Z# thing. Some library should define it
> as a standard platform compiler interface.

Anyway, this was just an example. The key point to look at is that the
compiler APIs (CG, LIR, etc..) don't care where COs come from or what
their exact, concrete types are. This means we can have ANY object to
participate in interactions, as long as SOME implementation knows how
to handle them (via capabilities), otherwise we'll just get an error.

> NOTE: Directly importing some `SRC -> CO` is actually not a good idea.
> See the `ors-importer.md` file for more information.
