using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Class
        : ICompileIRDefinitionAs<IR.Class>
        , ICompileIRType<OOPTypeReference<IR.Class>>
    {
        private IR.Class? IR { get; set; }

        Result<IR.Class> ICompileIRDefinitionAs<IR.Class>.CompileIRDefinition(Compiler.IR ir, object? target)
            => Result<IR.Class>.Ok(GetIR());

        Result<OOPTypeReference<IR.Class>> ICompileIRType<OOPTypeReference<IR.Class>>.CompileIRType(Compiler.IR ir)
            => Result<OOPTypeReference<IR.Class>>.Ok(new ClassReference(GetIR()));

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
