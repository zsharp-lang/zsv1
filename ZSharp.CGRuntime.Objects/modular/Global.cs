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

        public CGCode? Initializer { get; set; }

        public CGCode? Type { get; set; }

        IType ICTReadable.Type => IR!.Type;

        public Code Read(ICompiler compiler)
            => new([new IR.VM.GetGlobal(IR!)])
            {
                MaxStackSize = 1,
                Types = [IR!.Type],
            };
    }
}
