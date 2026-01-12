
namespace ZSharp.ZSSourceCompiler
{
    public sealed class DefaultContextCompiler(ZSSourceCompiler compiler)
        : CompilerBase(compiler)
        , IOverrideCompileExpression
        , IOverrideCompileStatement
    {
        public ObjectResult CompileNode(ZSSourceCompiler compiler, Expression node)
            => compiler.ExpressionCompiler.CompileNode(node);

        public ObjectResult CompileNode(ZSSourceCompiler compiler, Statement node)
            => compiler.StatementCompiler.CompileNode(node);
    }
}
