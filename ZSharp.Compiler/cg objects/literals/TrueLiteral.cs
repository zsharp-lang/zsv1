using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class TrueLiteral(CGObject type)
        : CGObject
        , ICTReadable
    {
        public CGObject Type { get; } = type;

        public Code Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.PutTrue()
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
