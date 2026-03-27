namespace ZSharp.SourceCompiler
{
    public sealed partial class DefinitionCompiler(
        Interpreter.Interpreter interpreter,
        Func<AST.Expression, IResult> compileExpression
    )
    {
        private readonly Compiler.Compiler compiler = interpreter.Compiler;

        private readonly Interpreter.Interpreter interpreter = interpreter;

        private Func<AST.Expression, IResult> CompileExpression { get; } = compileExpression;
    }
}
