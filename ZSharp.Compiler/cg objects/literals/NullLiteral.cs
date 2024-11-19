using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class NullLiteral(CGObject type) : Literal(null)
    {
        public override CGObject Type { get; } = type;

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
