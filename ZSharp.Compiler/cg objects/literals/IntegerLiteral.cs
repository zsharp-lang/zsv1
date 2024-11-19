using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class IntegerLiteral(DefaultIntegerType value, CGObject type)
        : Literal<DefaultIntegerType>(value)
        , ICTReadable
    {
        public override CGObject Type { get; } = type;

        public override Code Read(Compiler.Compiler compiler)
            => new(
                [
                    new IR.VM.PutInt32(Value)
                ]
            )
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
