using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    internal sealed class BoundMethod(Method method, CGObject instance)
        : CGObject
        , ICTCallable
    {
        public Method Method { get; } = method;

        public CGObject Instance { get; } = instance;

        public CGObject Call(Compiler.Compiler compiler, Argument[] arguments)
            => compiler.Call(Method, [new(Instance), .. arguments]);
    }
}
