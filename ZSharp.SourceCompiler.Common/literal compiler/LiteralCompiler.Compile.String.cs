namespace ZSharp.SourceCompiler
{
    partial class LiteralCompiler
    {
        private IResult CompileString(AST.LiteralExpression expression)
        {
            return Interpreter.RTLoader.Load(expression.Value);
        }
    }
}
