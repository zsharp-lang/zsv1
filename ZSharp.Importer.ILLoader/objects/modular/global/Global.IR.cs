using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Global
        : ICompileIRDefinitionAs<IR.Global>
    {
        public IR.Global? IR { get; private set; }

        IResult<IR.Global, Error> ICompileIRDefinitionAs<IR.Global>.CompileIRDefinition(Compiler.Compiler compiler, object? target)
        {
            if (IR is not null) return Result<IR.Global>.Ok(IR);

            if (target is not Platform.Runtime.Runtime rt)
                return Result<IR.Global>.Error("Invalid target platform");

            if (
                compiler.IR.CompileType(Type)
                .When(out var type)
                .Error(out var error)
            ) return Result<IR.Global>.Error(error);

            IR = new(Name, type!);

            rt.AddGlobal(IR, IL);

            return Result<IR.Global>.Ok(IR);
        }
    }
}
