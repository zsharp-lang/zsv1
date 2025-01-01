using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Parameter(string name) 
        : CompilerObject
        , ICTReadable
        , ICompileIRObject<IR.Parameter, IR.Signature>
    {
        public IR.Parameter? IR { get; set; }

        public string Name { get; } = name;

        public CompilerObject? Type { get; set; }

        public CompilerObject? Initializer { get; set; }

        public IR.Parameter CompileIRObject(Compiler.Compiler compiler, IR.Signature? owner)
        {
            if (Type is null)
                throw new("Parameter type not set.");

            IR ??= new(Name, compiler.CompileIRType(Type));

            return IR;
        }

        public IRCode Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.GetArgument(IR!),
            ])
            {
                MaxStackSize = 1,
                Types = [Type ?? throw new()]
            };
    }
}
