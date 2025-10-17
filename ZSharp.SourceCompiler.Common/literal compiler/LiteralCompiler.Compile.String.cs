namespace ZSharp.SourceCompiler
{
    partial class LiteralCompiler
    {
        private Result CompileString(AST.LiteralExpression expression)
        {
            return Interpreter.RTLoader.Load(expression.Value);
        }
    }
}
