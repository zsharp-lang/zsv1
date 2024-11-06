using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class Global(string name)
        : CGObject
        , ICTReadable
    {
        public IR.Global? IR { get; set; }

        public string Name { get; } = name;

        public bool IsReadOnly { get; set; }

        public CGObject? Initializer { get; set; }

        public CGObject? Type { get; set; }

        IType ICTReadable.Type => IR!.Type;

        public Code Read(Compiler.Compiler compiler)
            => new([new IR.VM.GetGlobal(IR!)])
            {
                MaxStackSize = 1,
                Types = [IR!.Type],
            };
    }
}
