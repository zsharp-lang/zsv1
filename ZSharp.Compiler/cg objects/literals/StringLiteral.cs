using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class StringLiteral(string value, CGObject type)
        : Literal<string>(value)
    {
        public override CGObject Type { get; } = type;

        public override Code Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.PutString(Value)
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
