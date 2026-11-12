namespace ZSharp.Compiler
{
    /// <summary>
    /// Registers how the nodes of <c>ZSharp.HIR.Code</c> map to CG operations.
    /// </summary>
    public static class CodeLoaders
    {
        /// <summary>
        /// What the code being loaded returns, if it returns anything: a return converts its
        /// value to it. Whatever lowers a function's code sets it while it loads that code,
        /// and puts back what it was after, since one function's code can load another's.
        /// </summary>
        public static CompilerObject? Returns { get; set; }

        private static readonly Dictionary<HIR.Instance, CompilerObject> variables = new(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Defines what a variable is to the code being loaded, as a parameter of the
        /// function whose code it is, or its object: whatever lowers a function's code
        /// defines its variables while it loads that code. Nothing else gives a variable a
        /// meaning in code, so code that uses a variable its function doesn't have fails to
        /// load. It gives what the variable was before, which <see cref="Restore"/> puts back
        /// once the code is loaded, since one function's code can load another's.
        /// </summary>
        public static CompilerObject? Define(HIR.Instance variable, CompilerObject value)
        {
            var previous = variables.GetValueOrDefault(variable);
            variables[variable] = value;
            return previous;
        }

        /// <summary>
        /// Puts back what a variable was before it was defined, or nothing.
        /// </summary>
        public static void Restore(HIR.Instance variable, CompilerObject? previous)
        {
            if (previous is null)
                variables.Remove(variable);
            else
                variables[variable] = previous;
        }

        /// <summary>
        /// What a variable is to the code being loaded, as its function defined it.
        /// </summary>
        public static IResult Variable(HIR.Instance variable)
            => variables.TryGetValue(variable, out var value)
                ? Result.Ok(value)
                : Result.Error("A parameter can only be used by the code it belongs to.");

        /// <summary>
        /// What a variable is to the code being loaded, or null.
        /// </summary>
        public static CompilerObject? DefinedAs(HIR.Instance variable)
            => variables.GetValueOrDefault(variable);

        /// <summary>
        /// A captured local lives where the functions that capture it can share it.
        /// </summary>
        private static CompilerObject Local(HIR.Code.Local local, CompilerObject type, CompilerObject? init)
            => local.IsCaptured ? new Objects.CellLocal(type, init) : new Objects.Local(type, init);

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

                return Result.Ok(OverloadGroup.Of(overloads));
            });

            instances.Types.Register<HIR.Infer>(_ => Result.Error("A type was meant to be inferred, but wasn't."));

            // The type of a value is what the compiler says it is: a value that has none, as
            // a definition with no type object, has no type to give.
            instances.Types.Register<HIR.Code.TypeOf>(node =>
                code.LoadExpression(node.Value).Error(out var error, out var value)
                    ? Result.Error(error)
                    : ZSharp.Compiler.Current.TS.TypeOf(value));

            instances.Definitions.Register<HIR.Code.Local>(local =>
            {
                CompilerObject? init = null;

                if (local.Init is not null && code.LoadExpression(local.Init).Error(out var error, out init))
                    return Result.Error(error);

                if (local.Type is not HIR.Infer)
                    return loader.LoadType(local.Type).Error(out error, out var declared)
                        ? Result.Error(error)
                        : Result.Ok(Local(local, declared, init));

                if (init is null)
                    return Result.Error($"Local {local.Name} has no initial value to infer its type from.");

                return ZSharp.Compiler.Current.TS.TypeOf(init).Error(out _, out var inferred)
                    ? Result.Error($"The type of local {local.Name} can't be inferred from its initial value.")
                    : Result.Ok(Local(local, inferred, init));
            });

            // A parameter as defined is what a call matches arguments to. As an instance it is
            // the variable of the code it belongs to, which whatever lowers that code gives.
            instances.Definitions.Register<HIR.Code.PositionalParameter>(parameter => Result.Ok(new Objects.ParameterMatch(
                parameter.Name, parameter.Type, parameter.DefaultValue, byName: false, byPosition: true
            )));
            instances.Definitions.Register<HIR.Code.NamedParameter>(parameter => Result.Ok(new Objects.ParameterMatch(
                parameter.Name, parameter.Type, parameter.DefaultValue, byName: true, byPosition: false
            )));

            // Outside the functions that capture it, a capture is what it captures.
            instances.Instances.Register<HIR.Code.Capture>(capture => capture.Captured is HIR.Instance captured
                ? loader.LoadInstance(captured)
                : loader.LoadDefinition(capture.Captured));
            instances.Instances.Register<HIR.Code.Local>(local => loader.LoadDefinition(local));

            expressions.Register<HIR.Code.Get>(get => loader.LoadInstance(get.Instance));

            expressions.Register<HIR.Code.GetValue>(get =>
                code.LoadExpression(get.Of).Error(out var error, out var value)
                    ? Result.Error(error)
                    : cg.Get(value));

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

            expressions.Register<HIR.Code.Match>(match =>
            {
                if (code.LoadExpression(match.Value).Error(out var error, out var value))
                    return Result.Error(error);

                if (code.LoadExpression(match.Pattern).Error(out error, out var pattern))
                    return Result.Error(error);

                return cg.Match(value, pattern).When(out var result).Error(out error)
                    ? Result.Error(error)
                    : Result.Ok(result!.Match);
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

            expressions.Register<HIR.Code.Index>(index =>
            {
                if (code.LoadExpression(index.Target).Error(out var error, out var target))
                    return Result.Error(error);

                var arguments = new List<Argument>();

                foreach (var argument in index.Arguments)
                    if (code.LoadExpression(argument.Value).Error(out error, out var value))
                        return Result.Error(error);
                    else
                        arguments.Add(new(argument.Name, value));

                return cg.Index(target, [.. arguments]);
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

            // A break or continue names its loop, which is loaded before the loop's body.
            var loops = new Dictionary<HIR.Code.Loop, Objects.Loop>(ReferenceEqualityComparer.Instance);

            mapped.Statements.Register<HIR.Code.While>(loop =>
            {
                var loaded = new Objects.WhileLoop();
                loops[loop] = loaded;

                if (code.LoadExpression(loop.Condition).Error(out var error, out var condition))
                    return Result.Error(error);

                if (code.LoadStatement(loop.Body).Error(out error, out var body))
                    return Result.Error(error);

                if (loop.Else is not null)
                    if (code.LoadStatement(loop.Else).Error(out error, out var otherwise))
                        return Result.Error(error);
                    else
                        loaded.Else = otherwise;

                loaded.Condition = condition;
                loaded.Body = body;

                return Result.Ok(loaded);
            });

            // The else part is loaded first, as it gives the loop its type.
            expressions.Register<HIR.Code.WhileExpression>(loop =>
            {
                if (code.LoadExpression(loop.Condition).Error(out var error, out var condition))
                    return Result.Error(error);

                if (code.LoadExpression(loop.Else).Error(out error, out var otherwise))
                    return Result.Error(error);

                if (ZSharp.Compiler.Current.TS.TypeOf(otherwise).Error(out _, out var type))
                    return Result.Error("The else part of a loop that is an expression has no type to give the loop.");

                var loaded = new Objects.WhileValue(type) { Condition = condition, Else = otherwise };
                loops[loop] = loaded;

                if (code.LoadStatement(loop.Body).Error(out error, out var body))
                    return Result.Error(error);

                loaded.Body = body;

                return Result.Ok(loaded);
            });

            mapped.Statements.Register<HIR.Code.Break>(leave =>
            {
                if (!loops.TryGetValue(leave.Loop, out var loop))
                    return Result.Error("A break leaves a loop that isn't loaded.");

                if (leave.Value is null)
                    return Result.Ok(new Objects.Break(loop, null));

                if (code.LoadExpression(leave.Value).Error(out var error, out var value))
                    return Result.Error(error);

                return cg.ImplicitCast(value, ((ITyped)loop).Type).Error(out error, out var converted)
                    ? Result.Error("The value a break leaves with doesn't match the type of the loop.")
                    : Result.Ok(new Objects.Break(loop, converted));
            });

            mapped.Statements.Register<HIR.Code.Continue>(next =>
                loops.TryGetValue(next.Loop, out var loop)
                    ? Result.Ok(new Objects.Continue(loop))
                    : Result.Error("A continue names a loop that isn't loaded.")
            );

            mapped.Statements.Register<HIR.Code.Return>(statement =>
            {
                if (statement.Value is null)
                    return Result.Ok(new Objects.Return(null));

                if (code.LoadExpression(statement.Value).Error(out var error, out var value))
                    return Result.Error(error);

                if (Returns is null)
                    return Result.Ok(new Objects.Return(value));

                return cg.ImplicitCast(value, Returns).Ok(out var converted)
                    ? Result.Ok(new Objects.Return(converted))
                    : Result.Error("A return value doesn't match what is returned.");
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
