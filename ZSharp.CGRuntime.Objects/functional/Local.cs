using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class Local 
        : CGObject
        , ICTReadable
    {
        public IR.VM.Local? IR { get; set; }

        public required string Name { get; set; }

        public CGCode? Type { get; set; }

        public CGCode? Initializer { get; set; }

        IType ICTReadable.Type => IR!.Type;

        public Code Read(ICompiler compiler)
            => new([new IR.VM.GetLocal(IR!)])
            {
                MaxStackSize = 1,
                Types = [IR!.Type]
            };
    }
}
