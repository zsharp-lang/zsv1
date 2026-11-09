namespace ZSharp.LIR
{
    /// <summary>
    /// A generic type given its type arguments. Its definition is any type that takes
    /// them, such as a handle to one a platform has, and is lowered once however many
    /// instances there are: the platform makes each instance of it.
    /// </summary>
    public sealed class GenericInstance(Type definition, params IEnumerable<Type> arguments)
        : Type
    {
        public Type Definition { get; set; } = definition;

        public List<Type> Arguments { get; } = [.. arguments];
    }

    /// <summary>
    /// A generic function given its type arguments, LIR types, which the platform makes
    /// the instance of. Its signature is the function's with the arguments in place of
    /// its type parameters.
    /// </summary>
    public sealed class MethodInstance(ICallable method, IEnumerable<Type> arguments, Signature signature)
        : ICallable
    {
        public ICallable Method { get; set; } = method;

        public List<Type> Arguments { get; } = [.. arguments];

        public Signature Signature { get; set; } = signature;
    }

    /// <summary>
    /// A member of a generic definition, called on an instance of it. Its signature is
    /// the member's as the instance has it, with the instance's arguments in place of
    /// the definition's type parameters.
    /// </summary>
    public sealed class InstanceMember(GenericInstance owner, ICallable member, Signature signature)
        : ICallable
    {
        public GenericInstance Owner { get; set; } = owner;

        public ICallable Member { get; set; } = member;

        public Signature Signature { get; set; } = signature;
    }
}
