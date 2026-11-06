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

            instances.Types.Register<HIR.Code.TypeExpression>(type => code.LoadExpression(type.Expression));

            instances.Definitions.Register<HIR.Code.Local>(local => Result.Ok(new Objects.Local(local)));
            instances.Instances.Register<HIR.Code.Local>(local => loader.LoadDefinition(local));

            expressions.Register<HIR.Code.Get>(get => loader.LoadInstance(get.Instance));

            expressions.Register<HIR.Code.Cast>(cast =>
            {
                if (code.LoadExpression(cast.Value).Error(out var error, out var value))
                    return Result.Error(error);

                return code.LoadExpression(cast.Type).Error(out error, out var type)
                    ? Result.Error(error)
                    : cg.Cast(value, type);
            });

            expressions.Register<HIR.Code.BlockExpression>(block =>
            {
                if (Block(block.Locals, block.Statements).Error(out var error, out var statements))
                    return Result.Error(error);

                return code.LoadExpression(block.Result).Error(out error, out var result)
                    ? Result.Error(error)
                    : Result.Ok(new Objects.BlockExpression((Objects.Block)statements, result));
            });

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

            mapped.Statements.Register<HIR.Code.Block>(block => Block(block.Locals, block.Statements));

            mapped.Statements.Register<HIR.Code.IfStatement>(statement =>
            {
                if (code.LoadExpression(statement.Condition).Error(out var error, out var condition))
                    return Result.Error(error);

                if (code.LoadStatement(statement.Then).Error(out error, out var then))
                    return Result.Error(error);

                if (statement.Else is null)
                    return Result.Ok(new Objects.If(condition, then, null));

                return code.LoadStatement(statement.Else).Error(out error, out var otherwise)
                    ? Result.Error(error)
                    : Result.Ok(new Objects.If(condition, then, otherwise));
            });

            mapped.Statements.Register<HIR.Code.Return>(statement =>
            {
                if (statement.Value is null)
                    return Result.Ok(new Objects.Return(null));

                return code.LoadExpression(statement.Value).Error(out var error, out var value)
                    ? Result.Error(error)
                    : Result.Ok(new Objects.Return(value));
            });

            IResult Block(List<HIR.Code.Local> locals, List<HIR.Code.Statement> statements)
            {
                var declared = new List<Objects.Local>();

                foreach (var local in locals)
                    if (loader.LoadDefinition(local).Error(out var error, out var value))
                        return Result.Error(error);
                    else
                        declared.Add((Objects.Local)value);

                var loaded = new List<CompilerObject>();

                foreach (var statement in statements)
                    if (code.LoadStatement(statement).Error(out var error, out var value))
                        return Result.Error(error);
                    else
                        loaded.Add(value);

                return Result.Ok(new Objects.Block([.. declared], [.. loaded]));
            }
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
