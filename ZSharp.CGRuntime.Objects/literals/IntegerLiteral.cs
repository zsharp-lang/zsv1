using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class IntegerLiteral(DefaultIntegerType value, IType type)
        : Literal<DefaultIntegerType>(value)
        , ICTReadable
    {
        public override IType Type { get; } = type;

        public override Code Read(ICompiler compiler)
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
