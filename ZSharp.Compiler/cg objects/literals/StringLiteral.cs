using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class StringLiteral(string value, IType type)
        : Literal<string>(value)
    {
        public override IType Type { get; } = type;

        public override IRCode Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.PutString(Value)
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
