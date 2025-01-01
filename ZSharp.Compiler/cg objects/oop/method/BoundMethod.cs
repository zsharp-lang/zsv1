using ZSharp.Compiler;

namespace ZSharp.Objects
{
    internal sealed class BoundMethod(Method method, CompilerObject instance)
        : CompilerObject
        , ICTCallable
    {
        public Method Method { get; } = method;

        public CompilerObject Instance { get; } = instance;

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
            => compiler.Call(Method, [new(Instance), .. arguments]);
    }
}
