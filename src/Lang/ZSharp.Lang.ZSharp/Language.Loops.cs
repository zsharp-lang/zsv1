namespace ZSharp.Lang
{
    /// <summary>
    /// The loop a <c>break</c> or <c>continue</c> in it can leave or continue: the
    /// innermost one, or the one it names with <c>from</c>.
    /// </summary>
    public sealed class LoopContext(Context? parent, string name, HIR.Code.Loop loop, bool hasValue)
        : ChildContext(parent)
    {
        public string Name { get; } = name;

        public HIR.Code.Loop Loop { get; } = loop;

        /// <summary>
        /// Whether the loop is an expression, so that leaving it gives its value.
        /// </summary>
        public bool HasValue { get; } = hasValue;

        /// <summary>
        /// The loop a context is in, the innermost one or the one with the name. A loop
        /// outside the definition the code belongs to can't be reached.
        /// </summary>
        public static LoopContext? Find(Context? context, string? name)
        {
            for (; context is ChildContext child and not DefinitionContext; context = child.Parent)
                if (context is LoopContext loop && (name is null || loop.Name == name))
                    return loop;

            return null;
        }
    }

    partial class Language
    {
        private static void InstallLoops(MappedASTCompiler mapped)
        {
            mapped.Expressions.Register<AST.WhileExpression<AST.Expression>>(Loop);
            mapped.Statements.Register<AST.BreakStatement>(Break);
            mapped.Statements.Register<AST.ContinueStatement>(Continue);
        }

        /// <summary>
        /// A loop statement. Its else part runs when the condition stops holding, and is
        /// outside the loop, so a break in it leaves an outer loop.
        /// </summary>
        private static IResult<HIRStatement, Error> Loop(ASTCompiler compiler, AST.WhileExpression<AST.Statement> node)
        {
            var loop = new HIR.Code.While();

            if (compiler.CompileExpression(node.Condition).When(out var condition).Error(out var error))
                return Result<HIRStatement>.Error(error);

            if (Body(compiler, node.Body, node.Name, loop, hasValue: false).When(out var body).Error(out error))
                return Result<HIRStatement>.Error(error);

            if (node.Else is not null)
                if (compiler.CompileStatement(node.Else).When(out var otherwise).Error(out error))
                    return Result<HIRStatement>.Error(error);
                else
                    loop.Else = otherwise;

            loop.Condition = condition!;
            loop.Body = body!;

            return Result<HIRStatement>.Ok(loop);
        }

        /// <summary>
        /// A loop expression. Its value is that of the break that leaves it, or that of its
        /// else part, so it must have one.
        /// </summary>
        private static IResult<HIRExpression, Error> Loop(ASTCompiler compiler, AST.WhileExpression<AST.Expression> node)
        {
            if (node.Else is null)
                return Result<HIRExpression>.Error("A loop that is an expression needs an else, for its value when it isn't left by a break.");

            var loop = new HIR.Code.WhileExpression();

            if (compiler.CompileExpression(node.Condition).When(out var condition).Error(out var error))
                return Result<HIRExpression>.Error(error);

            if (Body(compiler, node.Body, node.Name, loop, hasValue: true).When(out var body).Error(out error))
                return Result<HIRExpression>.Error(error);

            if (compiler.CompileExpression(node.Else).When(out var otherwise).Error(out error))
                return Result<HIRExpression>.Error(error);

            loop.Condition = condition!;
            loop.Body = body!;
            loop.Else = otherwise!;

            return Result<HIRExpression>.Ok(loop);
        }

        private static IResult<HIRStatement, Error> Body(ASTCompiler compiler, AST.Statement body, string name, HIR.Code.Loop loop, bool hasValue)
            => compiler.Contexts.Within(
                new LoopContext(compiler.Contexts.Current, name, loop, hasValue),
                () => compiler.CompileStatement(body)
            );

        private static IResult<HIRStatement, Error> Break(ASTCompiler compiler, AST.BreakStatement node)
        {
            if (LoopContext.Find(compiler.Contexts.Current, node.From) is not { } loop)
                return Result<HIRStatement>.Error(NoLoop("break", node.From));

            if (loop.HasValue && node.Value is null)
                return Result<HIRStatement>.Error("Leaving a loop that is an expression needs a value, as in break value.");

            if (!loop.HasValue && node.Value is not null)
                return Result<HIRStatement>.Error("Only a loop that is an expression can be left with a value.");

            var leave = new HIR.Code.Break(loop.Loop);

            if (node.Value is not null)
                if (compiler.CompileExpression(node.Value).When(out var value).Error(out var error))
                    return Result<HIRStatement>.Error(error);
                else
                    leave.Value = value;

            return Result<HIRStatement>.Ok(leave);
        }

        private static IResult<HIRStatement, Error> Continue(ASTCompiler compiler, AST.ContinueStatement node)
            => LoopContext.Find(compiler.Contexts.Current, node.From) is { } loop
                ? Result<HIRStatement>.Ok(new HIR.Code.Continue(loop.Loop))
                : Result<HIRStatement>.Error(NoLoop("continue", node.From));

        private static string NoLoop(string statement, string? name)
            => name is null
                ? $"A {statement} needs a loop around it."
                : $"There is no loop named {name} around this {statement}.";
    }
}
