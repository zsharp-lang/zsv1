namespace ZSharp.Compiler
{
    /// <summary>
    /// Registers how the statements of <c>ZSharp.HIR.Code</c> that need no lowering run.
    /// </summary>
    public static class CodeRunners
    {
        /// <summary>
        /// Registers what running means for code that nothing reified: an escape runs
        /// as it is loaded, and what it produced is loaded in its place.
        /// </summary>
        public static void Register(MappedCodeLoader code, HIRLoader loader, HIRRunner runner)
            => code.Expressions.Register<HIR.Code.Escape>(escape =>
                runner.EvaluateExpression(escape.Value).When(out var value).Error(out var error)
                    ? Result.Error(error)
                    : value is HIR.Instance instance
                        ? loader.LoadInstance(instance)
                        : Result.Error($"An escape must produce an instance, but got {value?.GetType().Name ?? "null"}.")
            );

        public static void Register(MappedRunner mapped, HIRRunner runner)
        {
            mapped.Register<HIR.Code.Block>(block =>
            {
                // Statements run one at a time here, so locals would not survive.
                if (block.Locals.Count > 0)
                    return new UnsupportedError("A block with locals runs as a whole.");

                foreach (var statement in block.Statements)
                    if (runner.RunStatement(statement) is { } error)
                        return error;

                return null;
            });

            mapped.Register<HIR.Code.IfStatement>(statement =>
            {
                if (runner.EvaluateExpression(statement.Condition).When(out var value).Error(out var error))
                    return error;

                if (value is not bool condition)
                    return new ErrorMessage($"A condition must be a boolean, but got {value?.GetType().Name ?? "null"}.");

                if (condition)
                    return runner.RunStatement(statement.Then);

                return statement.Else is null ? null : runner.RunStatement(statement.Else);
            });
        }
    }
}
