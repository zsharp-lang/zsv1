using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class RawType(IR.IType type)
        : CGObject
        //, ICTType
    {
        private readonly IR.IType type = type;

        public IR.IType AsType(ICompiler compiler)
            => type;
    }
}
