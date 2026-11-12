namespace ZSharp.Lang
{
    partial class DocumentCompiler
    {
        /// <summary>
        /// Runs a loop statement. Its body runs while its condition holds, and its else part
        /// runs when the condition stops holding, but not when a break leaves the loop. A
        /// statement in the loop is compiled the first time it is reached and runs on each
        /// iteration after that, and a block in it keeps its names, so what the first
        /// iteration defined is still defined.
        /// </summary>
        private Error? Loop(ASTCompiler compiler, AST.WhileExpression<AST.Statement> loop, Scope names)
        {
            var outermost = running.Count == 0;
            var self = new RunningLoop(loop.Name);

            if (outermost)
                remembered = new(ReferenceEqualityComparer.Instance);

            running.Push(self);

            try
            {
                while (true)
                {
                    if (Condition(compiler, loop.Condition, names, out var condition) is { } failure)
                        return failure;

                    if (!condition)
                        break;

                    switch (Run(compiler, loop.Body, names))
                    {
                        case null:
                            continue;
                        case Leaving { Next: true } next when next.Loop == self:
                            continue;
                        case Leaving { Next: false } leave when leave.Loop == self:
                            return null;
                        case { } error:
                            return error;
                    }
                }
            }
            finally
            {
                running.Pop();

                if (outermost)
                    remembered = null;
            }

            return loop.Else is null ? null : Run(compiler, loop.Else, names);
        }

        private Error? Condition(ASTCompiler compiler, AST.Expression expression, Scope names, out bool condition)
        {
            condition = false;

            if (Evaluate(compiler, expression, names).When(out var value).Error(out var error))
                return error;

            if (value is not bool result)
                return new ErrorMessage($"A condition must be a boolean, but got {value?.GetType().Name ?? "null"}.");

            condition = result;
            return null;
        }

        private IResult<object?, Error> Evaluate(ASTCompiler compiler, AST.Expression expression, Scope names)
        {
            if (Remembered(expression) is HIRExpression known)
                return host.Evaluate(Document, known);

            var context = new DocumentContext(this, names);

            return compiler.Contexts.Within(context, () =>
            {
                if (compiler.CompileExpression(expression).When(out var compiled).Error(out var error))
                    return Result<object?>.Error(error);

                foreach (var defined in context.Defined)
                    if (Run(defined) is { } failed)
                        return Result<object?>.Error(failed);

                Remember(expression, compiled!);

                return host.Evaluate(Document, compiled!);
            });
        }

        /// <summary>
        /// The loops running, innermost first.
        /// </summary>
        private readonly Stack<RunningLoop> running = [];

        /// <summary>
        /// While a loop runs, what each of its statements was compiled to, and the names of
        /// each of its blocks.
        /// </summary>
        private Dictionary<AST.Node, object>? remembered;

        private object? Remembered(AST.Node node)
            => remembered is not null && remembered.TryGetValue(node, out var known) ? known : null;

        private T Remembered<T>(AST.Node node, Func<T> make)
            where T : class
        {
            if (Remembered(node) is T known)
                return known;

            var made = make();
            Remember(node, made);
            return made;
        }

        private void Remember(AST.Node node, object value)
        {
            if (remembered is not null)
                remembered[node] = value;
        }

        private RunningLoop? Running(string? name)
            => name is null ? running.Peek() : running.FirstOrDefault(loop => loop.Name == name);

        private static ErrorMessage NoLoop(string statement, string? name)
            => new(name is null
                ? $"A {statement} needs a loop around it."
                : $"There is no loop named {name} around this {statement}.");

        private sealed class RunningLoop(string name)
        {
            public string Name { get; } = name;
        }

        /// <summary>
        /// A break or continue on its way to the loop it names. It is not an error, but it
        /// leaves every statement between them the way one does.
        /// </summary>
        private sealed class Leaving(RunningLoop loop, bool next)
            : ErrorMessage(next ? $"A continue of loop {loop.Name} got outside it." : $"A break of loop {loop.Name} got outside it.")
        {
            public RunningLoop Loop { get; } = loop;

            public bool Next { get; } = next;
        }
    }
}
