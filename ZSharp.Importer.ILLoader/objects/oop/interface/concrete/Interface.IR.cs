using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Interface
        : ICompileIRDefinitionAs<IR.Interface>
        , ICompileIRType<TypeReference<IR.Interface>>
    {
        public IR.Interface? IR { get; private set; }

        Result<IR.Interface> ICompileIRDefinitionAs<IR.Interface>.CompileIRDefinition(Compiler.Compiler compiler, object? target)
        {
            if (IR is not null) return Result<IR.Interface>.Ok(IR);

            if (target is not Platform.Runtime.Runtime rt)
                return Result<IR.Interface>.Error("Invalid target platform");

            IR = new(Name);

            rt.AddTypeDefinition(IR, IL);

            return Result<IR.Interface>.Ok(IR);
        }

        Result<TypeReference<IR.Interface>> ICompileIRType<TypeReference<IR.Interface>>.CompileIRType(Compiler.Compiler compiler)
        {
            if (
                compiler.IR.CompileDefinition<IR.Interface>(this, null)
                .When(out var definition)
                .Error(out var error)
            ) return Result<TypeReference<IR.Interface>>.Error(error);

            return Result<TypeReference<IR.Interface>>.Ok(new InterfaceReference(definition!));
        }
    }
}
