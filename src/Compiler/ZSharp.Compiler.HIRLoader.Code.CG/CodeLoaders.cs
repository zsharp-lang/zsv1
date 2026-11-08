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

            instances.Instances.Register<HIR.Code.OverloadGroup>(group =>
            {
                var overloads = new List<CompilerObject>();

                foreach (var item in group.Items)
                    if (loader.LoadInstance(item).Error(out var error, out var overload))
                        return Result.Error(error);
                    else
                        overloads.Add(overload);

                return Result.Ok(new Objects.OverloadGroup([.. overloads]));
            });

            instances.Types.Register<HIR.Infer>(_ => Result.Error("A type was meant to be inferred, but wasn't."));

            instances.Definitions.Register<HIR.Code.Local>(local =>
            {
                CompilerObject? init = null;

                if (local.Init is not null && code.LoadExpression(local.Init).Error(out var error, out init))
                    return Result.Error(error);

                if (local.Type is not HIR.Infer)
                    return loader.LoadType(local.Type).Error(out error, out var declared)
                        ? Result.Error(error)
                        : Result.Ok(new Objects.Local(declared, init));

                if (init is null)
                    return Result.Error($"Local {local.Name} has no initial value to infer its type from.");

                return Core.Compiler.ModuleScope.TS.TypeOf(init).Error(out _, out var inferred)
                    ? Result.Error($"The type of local {local.Name} can't be inferred from its initial value.")
                    : Result.Ok(new Objects.Local(inferred, init));
            });
            instances.Instances.Register<HIR.Code.Local>(local => loader.LoadDefinition(local));

            expressions.Register<HIR.Code.Get>(get => loader.LoadInstance(get.Instance));

            expressions.Register<HIR.Code.Unquote>(_ => Unquoted());
            mapped.Statements.Register<HIR.Code.UnquoteStatement>(_ => Unquoted());

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
                if (Block(block.Statements).Error(out var error, out var statements))
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

            mapped.Statements.Register<HIR.Code.Block>(block => Block(block.Statements));

            mapped.Statements.Register<HIR.Code.Declare>(declare =>
                loader.LoadDefinition(declare.Definition).Error(out var error, out var definition)
                    ? Result.Error(error)
                    : cg.Declare(definition)
            );

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

            static IResult Unquoted()
                => Result.Error("Unquoted code can only be inside code that is reified.");

            IResult Block(List<HIR.Code.Statement> statements)
            {
                var loaded = new List<CompilerObject>();

                foreach (var statement in statements)
                    if (code.LoadStatement(statement).Error(out var error, out var value))
                        return Result.Error(error);
                    else
                        loaded.Add(value);

                return Result.Ok(new Objects.Block([.. loaded]));
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
