using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class FalseLiteral(IType type)
        : CGObject
        , ICTReadable
    {
        public IType Type { get; } = type;

        public Code Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.PutFalse()
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
