using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class GenericInterface
        : ICompileIRDefinitionAs<IR.Interface>
        , ICompileIRType<TypeReference<IR.Interface>>
        , ICompileIRReference<TypeReference<IR.Interface>>
    {
        private IR.Interface? IR { get; set; }

        IResult<IR.Interface, Error> ICompileIRDefinitionAs<IR.Interface>.CompileIRDefinition(Compiler.Compiler compiler, object? target)
            => Result<IR.Interface>.Ok(GetIR());

        IResult<TypeReference<IR.Interface>, Error> ICompileIRReference<TypeReference<IR.Interface>>.CompileIRReference(Compiler.Compiler compiler, object? target)
            => Result<TypeReference<IR.Interface>>.Ok(new InterfaceReference(GetIR()));

        IResult<TypeReference<IR.Interface>, Error> ICompileIRType<TypeReference<IR.Interface>>.CompileIRType(Compiler.Compiler compiler, object? target)
            => Result<TypeReference<IR.Interface>>.Ok(new InterfaceReference(GetIR()));

        private IR.Interface GetIR()
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
