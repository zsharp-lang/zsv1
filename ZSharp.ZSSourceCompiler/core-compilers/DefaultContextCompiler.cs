
namespace ZSharp.ZSSourceCompiler
{
    public sealed class DefaultContextCompiler(ZSSourceCompiler compiler)
        : ContextCompiler(compiler)
        , IOverrideCompileExpression
        , IOverrideCompileStatement
    {
        public override Node ContextNode => throw new InvalidOperationException();

        public override CompilerObject ContextObject => throw new InvalidOperationException();

        public CompilerObject? CompileNode(ZSSourceCompiler compiler, Expression node)
            => compiler.ExpressionCompiler.CompileNode(node);

        public CompilerObject? CompileNode(ZSSourceCompiler compiler, Statement node)
            => compiler.StatementCompiler.CompileNode(node);

        public override CompilerObject CompileNode()
            => throw new InvalidOperationException();
    }
}
