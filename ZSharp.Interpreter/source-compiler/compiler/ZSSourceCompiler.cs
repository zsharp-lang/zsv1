using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ZSSourceCompiler
    {
        public Interpreter.Interpreter Interpreter { get; }

        public Compiler.Compiler Compiler => Interpreter.Compiler;

        public ExpressionCompiler ExpressionCompiler { get; }

        public StatementCompiler StatementCompiler { get; }

        public Context Context { get; }

        public ZSSourceCompiler(Interpreter.Interpreter interpreter)
        {
            Interpreter = interpreter;

            Context = new(this);

            ExpressionCompiler = new(this);
            StatementCompiler = new(this);

            Operators = Compiler.Feature<Ops>();

            StringImporter = new(interpreter);
            StandardLibraryImporter = new(interpreter);
            ZSImporter = new(interpreter);

            Initialize();
        }

        public ObjectResult CompileNode(Expression expression)
            => CompileNode<Expression>(expression);

        public ObjectResult CompileNode(Statement statement)
            => CompileNode<Statement>(statement);

        private ObjectResult CompileNode<T>(T node)
            where T : Node
        {
            foreach (var compiler in (IEnumerable<CompilerBase>)[
                Context.CurrentCompiler,
                Context.DefaultCompiler,
                ])
            {
                if (compiler is not IOverrideCompileNode<T> compileNode)
                    continue;

                var result = compileNode.CompileNode(this, node);

                if (result is null) continue;

                return result;
            }

            return CompilationError(
                "Could not compile node", node
            );
        }

        public TypeResult CompileType(Expression expression)
        {
            if (
                CompileNode(expression)
                .When(out var typeObject)
                .Error(out var error)
            )
                return TypeResult.Error(error);

            if (typeObject is IType type)
                return TypeResult.Ok(type);

            return CompilationError<IType>(
                "Dynamic type evaluation is not implemented yet", expression
            );
        }

        public Result<T, Error> CompilationError<T>(
            string error,
            Node node
        )
            where T : class
            => Result<T, Error>.Error(new CompilationError(node, error));

        public ObjectResult CompilationError(
            string error,
            Node node
        )
            => CompilationError<CompilerObject>(error, node);
    }
}
