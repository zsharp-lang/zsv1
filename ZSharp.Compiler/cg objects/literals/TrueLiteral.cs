using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class TrueLiteral(CompilerObject type)
        : CompilerObject
        , ICTReadable
    {
        public CompilerObject Type { get; } = type;

        public IRCode Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.PutTrue()
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
