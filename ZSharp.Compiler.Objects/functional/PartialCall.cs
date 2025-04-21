using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class PartialCall(CompilerObject target)
        : CompilerObject
        , ICTCallable
    {
        public CompilerObject Target { get; set; } = target;

        public Argument[] Arguments { get; set; } = [];

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            return compiler.Call(Target, [
                .. Arguments,
                .. arguments
            ]);
        }
    }
}
