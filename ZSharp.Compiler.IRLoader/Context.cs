using CommonZ.Utils;

namespace ZSharp.Compiler.IRLoader
{
    public sealed class Context
    {
        public Cache<IR.IRObject, CompilerObject> Objects { get; } = [];

        public Cache<IR.IType, CompilerObject> Types { get; } = [];
    }
}
