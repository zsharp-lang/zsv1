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

        public LIR.Variable Lowered => variable;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.GetLocal(variable));

        public IResult Set(CompilerObject value)
            => Core.Compiler.ModuleScope.CG.ImplicitCast(value, type()).Ok(out var converted)
                ? Result.Ok(new SetVariable(variable, converted, type))
                : Result.Error("The value doesn't match the type of this variable.");
    }

    internal sealed class SetVariable(LIR.Variable variable, CompilerObject value, Func<CompilerObject> type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type();

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Core.Compiler.ModuleScope.IR.CompileExpression(value, target).When(out var result).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Sequence(
                    new LIR.Block(new LIR.SetLocal(variable, result!)),
                    new LIR.GetLocal(variable)
                ));
    }
}
