using CommonZ.Utils;

namespace ZSharp.Compiler.IRLoader
{
    public sealed class Context
    {
        public Cache<ZSharp.IR.IRObject, CompilerObject> Objects { get; } = [];

        public Cache<ZSharp.IR.IType, IType> Types { get; } = [];
    }
}
