using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class FalseLiteral(CGObject type)
        : CGObject
        , ICTReadable
    {
        public CGObject Type { get; } = type;

        public Code Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.PutFalse()
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
