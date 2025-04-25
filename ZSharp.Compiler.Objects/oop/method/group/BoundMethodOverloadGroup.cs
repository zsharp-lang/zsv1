using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed  class BoundMethodOverloadGroup(MethodOverloadGroup group, CompilerObject instance)
        : CompilerObject
        , ICTCallable
    {
        public MethodOverloadGroup Group { get; } = group;

        public CompilerObject Instance { get; } = instance;

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
            => compiler.Call(Group, [
                new(Instance),
                .. arguments
            ]);
    }
}
