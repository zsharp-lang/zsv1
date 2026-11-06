namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A parameter or <c>this</c> of code being loaded.
    /// </summary>
    internal sealed class Variable(LIR.Variable variable, Func<CompilerObject> type)
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
    {
        CompilerObject ITyped.Type => type();

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.GetLocal(variable));

        public IResult Set(CompilerObject value)
            => Result.Ok(new SetVariable(variable, value));
    }

    internal sealed class SetVariable(LIR.Variable variable, CompilerObject value)
        : CompilerObject
        , ICompileIRExpression
    {
        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Core.Compiler.ModuleScope.IR.CompileExpression(value, target).When(out var result).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Sequence(
                    new LIR.Block(new LIR.SetLocal(variable, result!)),
                    new LIR.GetLocal(variable)
                ));
    }
}
