using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed  class BoundMethodOverloadGroup(MethodOverloadGroup group, CompilerObject instance)
        : CompilerObject
        , ICTCallable_Old
    {
        public MethodOverloadGroup Group { get; } = group;

        public CompilerObject Instance { get; } = instance;

        CompilerObject ICTCallable_Old.Call(Compiler.Compiler compiler, Argument[] arguments)
            => compiler.Call(Group, [
                new(Instance),
                .. arguments
            ]);
    }
}
