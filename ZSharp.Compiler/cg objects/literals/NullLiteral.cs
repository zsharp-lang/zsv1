using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class NullLiteral(IType type) : Literal(null)
    {
        public override IType Type { get; } = type;

        public override IRCode Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.PutNull()
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
