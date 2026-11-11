namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A parameter or <c>this</c> of code being loaded.
    /// </summary>
    /// <summary>
    /// A variable of code, as each target its code is lowered for lowers it: one CO for
    /// all of them, as what refers to it, as a local's initial value, is loaded once.
    /// </summary>
    internal sealed class Variable(Func<object?, LIR.Variable> variable, Func<CompilerObject> type)
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
    {
        public Variable(LIR.Variable variable, Func<CompilerObject> type)
            : this(_ => variable, type) { }

        CompilerObject ITyped.Type => type();

        public LIR.Variable For(object? target) => variable(target);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.GetLocal(variable(target)));

        public IResult Set(CompilerObject value)
            => ZSharp.Compiler.Current.CG.ImplicitCast(value, type()).Ok(out var converted)
                ? Result.Ok(new SetVariable(variable, converted, type))
                : Result.Error("The value doesn't match the type of this variable.");
    }

    internal sealed class SetVariable(Func<object?, LIR.Variable> variable, CompilerObject value, Func<CompilerObject> type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type();

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => ZSharp.Compiler.Current.IR.CompileExpression(value, target).When(out var result).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Sequence(
                    new LIR.Block(new LIR.SetLocal(variable(target), result!)),
                    new LIR.GetLocal(variable(target))
                ));
    }
}
