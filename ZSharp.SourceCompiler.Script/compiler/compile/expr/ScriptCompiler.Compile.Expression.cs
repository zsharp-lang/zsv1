using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private IResult Compile(AST.Expression expression)
            => PostProcess(
                expression switch
                {
                    AST.CallExpression call => Compile(call),
                    AST.IdentifierExpression identifier => Compile(identifier),
                    AST.LiteralExpression literal => Compile(literal),
                    AST.Module module => Compile(module),
                    _ => CompileExpression(expression)
                }
            );

        private IResult PostProcess(IResult result)
        {
            if (!result.Ok(out var value)) return result;

            if (Options.EvaluationTarget == EvaluationTarget.Statement)
                return result;

            return Interpreter.Evaluate(value).When<object?, Error, CompilerObject>(Interpreter.RTLoader.Load);
        }
    }
}
