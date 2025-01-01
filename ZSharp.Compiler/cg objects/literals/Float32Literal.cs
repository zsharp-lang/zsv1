using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Float32Literal(float value, CompilerObject type)
        : Literal<float>(value)
        , ICTReadable
    {
        public override CompilerObject Type { get; } = type;

        public override IRCode Read(Compiler.Compiler compiler)
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
