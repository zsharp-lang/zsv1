namespace ZSharp.Compiler
{
    /// <summary>
    /// Implements <see cref="HIRRunner"/>. A statement with a registered runner uses
    /// it, which is how control flow runs and how extensions add statements that act
    /// at compile time. Anything else is loaded and handed to the functions given
    /// here, which decide how code actually executes.
    /// </summary>
    public sealed class MappedRunner(
        HIRRunner runner,
        HIRCodeLoader loader,
        Func<CompilerObject, Error?> execute,
        Func<CompilerObject, IResult<object?, Error>> evaluate
    )
    {
        private readonly HIRRunner runner = runner;
        private readonly Dictionary<Type, RunStatement> statements = [];

        private HIRRunner @base = null!;

        public void Apply()
        {
            @base = runner.Clone();

            runner.RunStatement = RunStatement;
            runner.EvaluateExpression = EvaluateExpression;
        }

        /// <summary>
        /// Registers a runner, replacing any existing one for that type.
        /// </summary>
        public void Register<T>(Func<T, Error?> run)
            where T : HIR.Code.Statement
            => statements[typeof(T)] = statement => run((T)statement);

        private Error? RunStatement(HIR.Code.Statement statement)
        {
            var error = @base.RunStatement(statement);

            if (!error.IsUnsupported())
                return error;

            if (
                statements.TryGetValue(statement.GetType(), out var run)
                && !(error = run(statement)).IsUnsupported()
            ) return error;

            return loader.LoadStatement(statement)
                .When(out var @object)
                .Error(out error)
                    ? error
                    : execute(@object!);
        }

        private IResult<object?, Error> EvaluateExpression(HIR.Code.Expression expression)
        {
            var result = @base.EvaluateExpression(expression);

            if (!result.IsUnsupported())
                return result;

            return loader.LoadExpression(expression)
                .When(out var @object)
                .Error(out var error)
                    ? Result<object?>.Error(error)
                    : evaluate(@object!);
        }
    }
}
