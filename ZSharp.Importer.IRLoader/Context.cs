using CommonZ.Utils;

namespace ZSharp.Importer.IRLoader
{
    public sealed class Context
    {
        public Cache<ZSharp.IR.IRDefinition, CompilerObject> Objects { get; } = [];

        public Cache<ZSharp.IR.IType, CompilerObject> Types { get; } = [];
    }
}
