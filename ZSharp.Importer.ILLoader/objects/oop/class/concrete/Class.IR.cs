using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Class
        : ICompileIRDefinitionAs<IR.Class>
        , ICompileIRType<TypeReference<IR.Class>>
        , ICompileIRReference<TypeReference<IR.Class>>
    {
        private IR.Class? IR { get; set; }

        IResult<IR.Class, Error> ICompileIRDefinitionAs<IR.Class>.CompileIRDefinition(Compiler.Compiler compiler, object? target)
            => Result<IR.Class>.Ok(GetIR());

        IResult<TypeReference<IR.Class>, Error> ICompileIRReference<TypeReference<IR.Class>>.CompileIRReference(Compiler.Compiler compiler, object? target)
            => Result<TypeReference<IR.Class>>.Ok(new ClassReference(GetIR()));

        IResult<TypeReference<IR.Class>, Error> ICompileIRType<TypeReference<IR.Class>>.CompileIRType(Compiler.Compiler compiler, object? target)
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
