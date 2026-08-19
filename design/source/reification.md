# Reification

In Python, when a function body contains a class, the class
object is constructed anew with every invocation of the function.
This allows for very powerful code-generation capabilities.

In C#, a class inside a function body can only mean the class
is private to that function, but the class itself has a single
identity.

Both approaches have their strengths, and Z# want it all.

If the type definition is cast to its meta-type, it is *not* an
identity, and *cannot* be used as a type. Otherwise, regardless
of where in an expression a type definition appears, it is an
identity.

Let's see:
```
fun createMatrixType(w: I32, h: I32?, /* keyword-only argument */ { type: Type /* <- This is ORS type! */ = F32 }): Class {
    h = h or w;

    let matrixType = class Matrix of Class {
        @class // class field, similar to Pyton's ClassVar
        let width = w;

        @class
        let height = h;
    } as Class; // This cast means this is reflection/code-generation only!

    matrixType.name += "${w}x${h}";
    for (let j in range(h)) for (let i in range(w))
        matrixType.fields.add(Field("m${i}${j}, type));

    return matrixType;
}
```

Calling this function does NOT create a new type. It simply constructs
a new `Class` object. More specifically, this cannot be used directly
as a type.

```
let Matrix4x4 = createMatrixType(4);
let myMatrix: Matrix4x4; // <- this is not valid!!
```

However, we can do this:
```
let cls = Class("MyClass"); // ok so far, just a new class.
cls.Field("myMatrix", Matrix4x4); // ok! this is just a `Type` since `Class : Type`
```

If we want to actually use it as a type we'll need to load it
into the runtime. This can be done by evaluating the `Type` object,
which results with the `TypeObject` of that type.
In our case, evaluating `Matrix4x4` causes the system to import it
into the OIS system, then use `CG.Get`, then compile it down to LIR,
which is then loaded into the runtime and evaluated to produce a the
singleton `Class[Matrix4x4]` object, which holds the static and class
attributes (`width`, `height`).
