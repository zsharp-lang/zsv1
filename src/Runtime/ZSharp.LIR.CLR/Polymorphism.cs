namespace ZSharp.LIR.CLR
{
    /// <summary>
    /// A CLR platform's mark on a structure whose values are objects of a CLR class, or
    /// whose values are objects of any class that implements it, if it is an interface.
    /// What marks it is the capability of polymorphism every CLR platform has.
    /// </summary>
    public sealed class Polymorphic
        : LIR.Metadata
    {
        public bool IsInterface { get; set; }

        /// <summary>
        /// The interfaces the class implements, or the interface extends.
        /// </summary>
        public List<LIR.Structure> Interfaces { get; } = [];

        /// <summary>
        /// The CLR's own types the class implements, as an interface of the toolchain's,
        /// which a CLR platform gives the class it realizes.
        /// </summary>
        public List<LIR.Type> Implements { get; } = [];
    }

    /// <summary>
    /// A CLR platform's mark on a function that is a method of a polymorphic structure:
    /// it takes the object first, and calling it calls what the object's class has.
    /// </summary>
    public sealed class MethodOf(LIR.Structure owner)
        : LIR.Metadata
    {
        public LIR.Structure Owner { get; } = owner;
    }

    /// <summary>
    /// A CLR platform's mark on a function with no code that casts an object down to a
    /// type it might be of, failing if it isn't. Calling it is the cast.
    /// </summary>
    public sealed class CastDown(LIR.Type target)
        : LIR.Metadata
    {
        public LIR.Type Target { get; } = target;
    }

    /// <summary>
    /// A CLR platform's mark on a function with no code that tells whether an object is of
    /// a type. Calling it is the test, and <c>null</c> is of no type.
    /// </summary>
    public sealed class TypeTest(LIR.Type target)
        : LIR.Metadata
    {
        public LIR.Type Target { get; } = target;
    }
}
