using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Local 
        : CGObject
        , ICTReadable
    {
        public IR.VM.Local? IR { get; set; }

        public required string Name { get; set; }

        public CGObject? Type { get; set; }

        public CGObject? Initializer { get; set; }

        public Code Read(Compiler.Compiler compiler)
            => new([new IR.VM.GetLocal(IR!)])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
