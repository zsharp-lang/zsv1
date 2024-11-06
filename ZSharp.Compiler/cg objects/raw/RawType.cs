using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class RawType(IType type)
        : CGObject
        , ICTReadable
        //, ICTType
    {
        private readonly IType type = type;

        public IType Type => throw new NotImplementedException();

        public IType AsType(Compiler.Compiler compiler)
            => type;

        public Code Read(Compiler.Compiler compiler)
            => type is IRObject ir ? new([
                new IR.VM.GetObject(ir)
            ])
            {
                MaxStackSize = 1,
                Types = [type]
            } : throw new();
    }
}
