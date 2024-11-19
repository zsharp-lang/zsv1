using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Float32Literal(float value, CGObject type)
        : Literal<float>(value)
        , ICTReadable
    {
        public override CGObject Type { get; } = type;

        public override Code Read(Compiler.Compiler compiler)
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
