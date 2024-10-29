using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class StringLiteral(string value, IType type)
        : Literal<string>(value)
    {
        public override IType Type { get; } = type;

        public override Code Read(ICompiler compiler)
            => new([
                new IR.VM.PutString(Value)
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
