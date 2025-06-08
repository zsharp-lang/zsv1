using ZSharp.Compiler;
using ZSharp.Objects;

namespace ZSharp.Interpreter
{
    internal sealed class ZSSourceEvaluator(ZSSourceCompiler.ZSSourceCompiler sourceCompiler) : Compiler.Evaluator
    {
        private readonly ZSSourceCompiler.ZSSourceCompiler sourceCompiler = sourceCompiler;

        public override Result<CompilerObject, string> Evaluate(CompilerObject @object)
        {
            if (@object is not ZSSourceCompiler.NodeObject nodeObject)
                return Result<CompilerObject, string>.Ok(@object);

            if (nodeObject.Node is AST.Expression expression)
                return Result<CompilerObject, string>.Ok(
                    sourceCompiler.CompileNode(expression).Unwrap()
                );
            if (nodeObject.Node is AST.Statement statement)
                return Result<CompilerObject, string>.Ok(
                    sourceCompiler.CompileNode(statement).Unwrap()
                );

            return Result<CompilerObject, string>.Ok(@object);
        }
    }
}
