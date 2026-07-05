using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private IResult<object, Error> Evaluate(AST.Expression expression)
        {
            if (
                Compile(expression)
                .When(out var compiledExpression)
                .Error(out var error)
            ) return Result<object>.Error(error);

            return Interpreter.Evaluate(compiledExpression!)!;
        }
    }
}
