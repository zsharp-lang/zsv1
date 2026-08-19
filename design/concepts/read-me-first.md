# Introducion


Thid directory contains descriptions of different concepts that Z# has. However, it
is important to understand that these concepts are NOT mapped to any C# entity.
These are purely design concepts, and have nothing to do with implementation.

That is, if, for example, we talk about the concept of an ORS importer, that doesn't
mean there's some `IORSImporter` interface. On the contrary! Since it is defined in
this directory, it must mean it's a conceptual idea and NOT a concrete type / interface.

I can't stress enough how these concepts are completely separate from code. They are
pure ideas, nothing more. So, when I say something is an X, and that X is a concept that
is defined here, it most certainly means it MUST NOT implement some X interface.


The motivation behind this is because these concepts are more of an convention rather
than a language defined API. The language should care exactly 0 care-points about ideas
and concepts defined here, and thus there is no reason whatsoever to define an interface
for such concepts.
