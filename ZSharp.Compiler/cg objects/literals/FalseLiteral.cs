using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class FalseLiteral(CompilerObject type)
        : CompilerObject
        , ICTReadable
    {
        public CompilerObject Type { get; } = type;

        public IRCode Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.PutFalse()
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
