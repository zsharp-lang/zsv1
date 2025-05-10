using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class BoundGenericMethodReference(GenericMethodReference method, CompilerObject instance)
        : CompilerObject
        , ICTCallable
    {
        public GenericMethodReference Method { get; } = method;

        public CompilerObject Instance { get; } = instance;

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
            => compiler.Call(Method, [new(Instance), .. arguments]);
    }
}
