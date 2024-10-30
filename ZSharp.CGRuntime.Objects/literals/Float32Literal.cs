using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class Float32Literal(float value, IType type)
        : Literal<float>(value)
        , ICTReadable
    {
        public override IType Type { get; } = type;

        public override Code Read(ICompiler compiler)
            => new(
                [
                    new IR.VM.PutFloat32(Value)
                ]
            )
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
