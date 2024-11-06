using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class NullLiteral(IType type) : Literal(null)
    {
        public override IType Type { get; } = type;

        public override Code Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.PutNull()
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
