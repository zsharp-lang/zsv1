using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class IntegerLiteral(DefaultIntegerType value, IType type)
        : Literal<DefaultIntegerType>(value)
        , ICTReadable
    {
        public override IType Type { get; } = type;

        public override IRCode Read(Compiler.Compiler compiler)
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
