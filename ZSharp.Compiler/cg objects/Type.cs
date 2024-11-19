using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    internal sealed class Type(IRType type)
        : CGObject
        , ICTReadable
    {
        private readonly IRType type = type;
        private readonly IRObject ir = type as IRObject ?? throw new();

        CGObject ICTReadable.Type => this;

        public Code Read(Compiler.Compiler compiler)
        => new([
                new IR.VM.GetObject(ir)
            ])
            {
                MaxStackSize = 1,
                Types = [this]
            };
    }
}
