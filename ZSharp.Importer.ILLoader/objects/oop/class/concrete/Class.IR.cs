using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Class
        : ICompileIRDefinitionAs<IR.Class>
        , ICompileIRType<TypeReference<IR.Class>>
    {
        private IR.Class? IR { get; set; }

        Result<IR.Class> ICompileIRDefinitionAs<IR.Class>.CompileIRDefinition(Compiler.Compiler compiler, object? target)
            => Result<IR.Class>.Ok(GetIR());

        Result<TypeReference<IR.Class>> ICompileIRType<TypeReference<IR.Class>>.CompileIRType(Compiler.Compiler compiler)
            => Result<TypeReference<IR.Class>>.Ok(new ClassReference(GetIR()));

        private IR.Class GetIR()
        {
            if (IR is null)
            {
                IR = new(IL.Name);
                Loader.RequireRuntime().AddTypeDefinition(IR, IL);
            }

            return IR;
        }
    }
}
