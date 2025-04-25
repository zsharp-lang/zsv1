using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class FalseLiteral(IType type)
        : CompilerObject
        , ICTReadable
    {
        public IType Type { get; } = type;

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
