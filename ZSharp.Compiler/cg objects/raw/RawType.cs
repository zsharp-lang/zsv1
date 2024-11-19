using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class RawType(IRType type, CGObject metaType)
        : CGObject
        , ICTReadable
        //, ICTType
    {
        private IRType type = type;

        public CGObject Type { get; internal set; } = metaType;

        public IType AsType(Compiler.Compiler compiler)
            => type;

        public Code Read(Compiler.Compiler compiler)
            => type is IRObject ir ? new([
                new IR.VM.GetObject(ir)
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            } : throw new();
    }
}
