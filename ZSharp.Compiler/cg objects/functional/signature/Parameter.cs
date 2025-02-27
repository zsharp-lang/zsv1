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

            IR ??= new(Name, compiler.RuntimeModule.TypeSystem.Void);
            if (IR.Type == compiler.RuntimeModule.TypeSystem.Void)
                IR.Type = compiler.CompileIRType(Type);

            return IR;
        }

        public IRCode Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.GetArgument(IR ?? compiler.CompileIRObject<IR.Parameter, IR.Signature>(this, null)),
            ])
            {
                MaxStackSize = 1,
                Types = [Type ?? throw new()]
            };
    }
}
