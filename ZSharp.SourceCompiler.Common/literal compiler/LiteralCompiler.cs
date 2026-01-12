namespace ZSharp.SourceCompiler
{
    public sealed partial class LiteralCompiler(Interpreter.Interpreter interpreter)
    {
        public Interpreter.Interpreter Interpreter { get; } = interpreter;
    }
}
