namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ZSSourceCompiler : Compiler.Feature
    {
        public ExpressionCompiler ExpressionCompiler { get; }

        public StatementCompiler StatementCompiler { get; }

        public Context Context { get; }

        public ZSSourceCompiler(Compiler.Compiler compiler)
            : base(compiler)
        {
            Context = new(this);

            ExpressionCompiler = new(this);
            StatementCompiler = new(this);

            Initialize();
        }

        public CompilerObject CompileNode(Expression expression)
            => CompileNode<Expression>(expression);

        public CompilerObject CompileNode(Statement statement)
            => CompileNode<Statement>(statement);

        private CompilerObject CompileNode<T>(T node)
            where T : Node
        {
            foreach (var compiler in Context.Compilers<IOverrideCompileNode<T>>())
                if (compiler.CompileNode(this, node) is CompilerObject result)
                    return result;

            throw new($"Could not find suitable compiler for node of type {node.GetType().Name}"); // TODO: proper exception: could not find suitable compiler for T
        }

        public CompilerObject CompileType(Expression expression)
            => Compiler.TypeSystem.EvaluateType(CompileNode(expression));
    }
}
