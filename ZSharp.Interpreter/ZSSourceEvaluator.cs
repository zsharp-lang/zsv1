using ZSharp.Objects;

namespace ZSharp.Interpreter
{
    internal sealed class ZSSourceEvaluator(ZSSourceCompiler.ZSSourceCompiler sourceCompiler) : Compiler.Evaluator
    {
        private readonly ZSSourceCompiler.ZSSourceCompiler sourceCompiler = sourceCompiler;

        public override CompilerObject Evaluate(CompilerObject @object)
        {
            if (@object is not ZSSourceCompiler.NodeObject nodeObject) return @object;

            if (nodeObject.Node is AST.Expression expression)
                return sourceCompiler.CompileNode(expression);
            if (nodeObject.Node is AST.Statement statement)
                return sourceCompiler.CompileNode(statement);

            return @object;
        }
    }
}
