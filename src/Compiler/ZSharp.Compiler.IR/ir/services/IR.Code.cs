namespace ZSharp.Compiler
{
    public delegate IResult<LIR.Expression, Error> CompileExpression(CompilerObject @object, object? target);

    public delegate IResult<LIR.Statement, Error> CompileStatement(CompilerObject @object, object? target);

    partial class IR
    {
        public CompileExpression CompileExpression { get; set; } = Dispatcher.Instance.CompileExpression;

        public CompileStatement CompileStatement { get; set; } = Dispatcher.Instance.CompileStatement;
    }
}
