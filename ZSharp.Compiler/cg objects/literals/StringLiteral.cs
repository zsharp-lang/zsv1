using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class StringLiteral(string value, CompilerObject type)
        : Literal<string>(value)
    {
        public override CompilerObject Type { get; } = type;

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
