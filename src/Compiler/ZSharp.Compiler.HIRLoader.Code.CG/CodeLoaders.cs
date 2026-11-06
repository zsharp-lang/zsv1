namespace ZSharp.Compiler
{
    /// <summary>
    /// Registers how the nodes of <c>ZSharp.HIR.Code</c> map to CG operations.
    /// </summary>
    public static class CodeLoaders
    {
        public static void Register(MappedCodeLoader mapped, MappedLoader instances, HIRCodeLoader code, HIRLoader loader, CG cg)
        {
            var expressions = mapped.Expressions;

            instances.Instances.Register<HIR.Code.Reference>(reference =>
                reference.Value is null
                    ? Result.Error("A reference was used before it was set.")
                    : loader.LoadInstance(reference.Value)
            );

            expressions.Register<HIR.Code.Get>(get => loader.LoadInstance(get.Instance));

            expressions.Register<HIR.Code.NamedMember>(member =>
                code.LoadExpression(member.Target).Error(out var error, out var target)
                    ? Result.Error(error)
                    : cg.Member(target, member.Name)
            );

            expressions.Register<HIR.Code.Call>(call =>
            {
                if (code.LoadExpression(call.Callee).Error(out var error, out var callee))
                    return Result.Error(error);

                var arguments = new List<Argument>();

                foreach (var argument in call.Arguments)
                    if (code.LoadExpression(argument.Value).Error(out error, out var value))
                        return Result.Error(error);
                    else
                        arguments.Add(new(argument.Name, value));

                return cg.Call(callee, [.. arguments]);
            });

            expressions.Register<HIR.Code.Set>(set =>
            {
                if (code.LoadExpression(set.Target).Error(out var error, out var target))
                    return Result.Error(error);

                return code.LoadExpression(set.Value).Error(out error, out var value)
                    ? Result.Error(error)
                    : cg.Set(target, value);
            });

            mapped.Statements.Register<HIR.Code.ExpressionStatement>(
                statement => code.LoadExpression(statement.Expression)
            );
        }

        private static bool Error(
            this IResult result,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Error? error,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out CompilerObject? value
        )
        {
            value = result.IsOk ? result.Unwrap() : null;
            error = result.IsError ? result.UnwrapError() : null;

            return result.IsError;
        }
    }
}
