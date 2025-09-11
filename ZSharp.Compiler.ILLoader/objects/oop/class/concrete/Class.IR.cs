using ZSharp.IR;

namespace ZSharp.Compiler.ILLoader.Objects
{
    partial class Class
        : ICompileIRDefinitionAs<ZSharp.IR.Class>
        , ICompileIRType<OOPTypeReference<ZSharp.IR.Class>>
    {
        private ZSharp.IR.Class? IR { get; set; }

        Result<ZSharp.IR.Class> ICompileIRDefinitionAs<ZSharp.IR.Class>.CompileIRDefinition(IR ir, object? target)
            => Result<ZSharp.IR.Class>.Ok(GetIR());

        Result<OOPTypeReference<ZSharp.IR.Class>> ICompileIRType<OOPTypeReference<ZSharp.IR.Class>>.CompileIRType(IR ir)
            => Result<OOPTypeReference<ZSharp.IR.Class>>.Ok(new ClassReference(GetIR()));

        private ZSharp.IR.Class GetIR()
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
