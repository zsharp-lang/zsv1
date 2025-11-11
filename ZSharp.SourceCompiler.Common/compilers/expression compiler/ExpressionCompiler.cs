namespace ZSharp.SourceCompiler
{
    public sealed partial class ExpressionCompiler(Interpreter.Interpreter interpreter)
    {
        public Interpreter.Interpreter Interpreter { get; } = interpreter;
    }
}
