using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Parameter(string name) 
        : CGObject
        , ICTReadable
    {
        public IR.Parameter? IR { get; set; }

        public string Name { get; } = name;

        public CGObject? Type { get; set; }

        public CGObject? Initializer { get; set; }

        public Code Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.GetArgument(IR!),
            ])
            {
                MaxStackSize = 1,
                Types = [Type ?? throw new()]
            };
    }
}
