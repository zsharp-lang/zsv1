namespace ZSharp.SourceCompiler.Module
{
    partial class FunctionBodyCompiler
    {
        private IResult Compile(AST.Return @return)
        {
            CompilerObject? value = null;

            if (
                @return.Value is not null &&
                CompileExpression(@return.Value)
                .When(out value)
                .Error(out var error)
            ) return Result.Error(error);

            return Result.Ok(new Return() { Value = value } );
        }
    }
}
