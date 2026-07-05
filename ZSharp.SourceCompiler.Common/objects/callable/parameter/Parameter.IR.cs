using IRParameterResult = ZSharp.Compiler.Result<ZSharp.IR.Parameter>;

namespace ZSharp.SourceCompiler.Objects
{
    partial class Parameter
        : ICompileIRDefinitionAs<IR.Parameter>
    {
        private IR.Parameter? IR { get; set; }

        IResult<IR.Parameter, Error> ICompileIRDefinitionAs<IR.Parameter>.CompileIRDefinition(ZSharp.Compiler.Compiler compiler, object? target)
        {
            if (IR is not null)
                return IRParameterResult.Ok(IR);

            if (
                compiler.IR.CompileType(Type, target)
                .When(out var type)
                .Error(out var error)
            ) return IRParameterResult.Error(error);

            IR = new(Name, type!);

            return IRParameterResult.Ok(IR);
        }
    }
}
