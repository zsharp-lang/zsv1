namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed class Return(CompilerObject value)
        : CompilerObject
        , ICompileIRStatement
    {
        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
            => Core.Compiler.ModuleScope.IR.CompileExpression(value, target).When(out var result).Error(out var error)
                ? Result<LIR.Statement>.Error(error)
                : Result<LIR.Statement>.Ok(new LIR.Return(result));
    }
}
