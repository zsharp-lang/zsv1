namespace ZSharp.Lang
{
    /// <summary>
    /// The built-in Z# language. It is an extension like any other: everything it does
    /// is registered here. It keeps no context of its own: each handler compiles with the
    /// compiler it runs in, and asks that compiler's contexts for what it needs.
    /// </summary>
    public sealed class Language(Binder binder)
    {
        private Func<HIRExpression, HIR.Instance, bool> isUnderlyingType = null!;

        /// <summary>
        /// The locals that <c>let</c> declared, as opposed to the locals holding
        /// definitions, which are never overridden.
        /// </summary>
        private readonly HashSet<HIR.Code.Local> lets = [];

        /// <param name="isUnderlyingType">
        /// Whether code that evaluates to a type is the given definition type.
        /// </param>
        public void Install(
            MappedASTCompiler mapped,
            MappedBinder binding,
            Func<HIRExpression, HIR.Instance, bool> isUnderlyingType
        )
        {
            this.isUnderlyingType = isUnderlyingType;

            // A let bound over another let overrides it, which is how a let reuses a name.
            // The local holding a definition is never overridden.
            binding.Merges.Add((existing, value) =>
                existing is HIR.Code.Get { Instance: HIR.Code.Local before } && lets.Contains(before)
                && value is HIR.Code.Get { Instance: HIR.Code.Local after } && lets.Contains(after)
                    ? value
                    : null
            );

            mapped.Expressions.Register<AST.IdentifierExpression>(Compile);
            mapped.Expressions.Register<AST.LiteralExpression>(Compile);
            mapped.Expressions.Register<AST.CallExpression>(Compile);
            mapped.Expressions.Register<AST.BinaryExpression>(Compile);
            mapped.Expressions.Register<AST.CastExpression>(Compile);
            mapped.Expressions.Register<AST.LetExpression>(Compile);
            mapped.Expressions.Register<AST.Function>(CompileDefinition);
            mapped.Expressions.Register<AST.TypeDefinition>(CompileDefinition);
            mapped.Expressions.Register<AST.Module>(CompileDefinition);

            mapped.Statements.Register<AST.ExpressionStatement>(Compile);
            mapped.Statements.Register<AST.DefinitionStatement>(Compile);
            mapped.Statements.Register<AST.DecoratedStatement>(Compile);
            mapped.Statements.Register<AST.BlockStatement>(Compile);
            mapped.Statements.Register<AST.IfStatement>(Compile);
            mapped.Statements.Register<AST.ImportStatement>(Compile);
            mapped.Statements.Register<AST.Return>(Compile);
        }

        /// <param name="import">
        /// Imports from the value an import source evaluates to. A source is an
        /// expression, so that value can be anything.
        /// </param>
        /// <param name="load">
        /// Loads a definition that was just built, returning why it could not be.
        /// </param>
        public static void Install(
            MappedRunner mapped,
            HIRRunner runner,
            Func<object?, IResult<HIR.Instance, Error>> import,
            Func<HIR.Instance, Error?> load
        )
        {
            mapped.Register<Import>(statement =>
            {
                if (runner.EvaluateExpression(statement.Source).When(out var source).Error(out var error))
                    return error;

                if (import(source).When(out var result).Error(out error))
                    return error;

                statement.Result.Value = result;

                return null;
            });

            mapped.Register<Define>(statement =>
            {
                if (runner.EvaluateExpression(statement.Build).When(out var value).Error(out var error))
                    return error;

                if (value is not HIR.Instance instance)
                    return new ErrorMessage($"A definition must produce an instance, but got {value?.GetType().Name ?? "null"}.");

                statement.Result.Value = instance;

                // A definition that can't load is a failure, not an unsupported statement.
                return load(instance) is { } failure ? new ErrorMessage($"{failure}") : null;
            });
        }

        /// <summary>
        /// The names of the context the compiler is in, if it has any.
        /// </summary>
        public static Scope? Names(ASTCompiler compiler)
            => compiler.Contexts.Has<INames>(out var names) ? names.Names : null;

        /// <summary>
        /// Defines a name in the names of the context the compiler is in.
        /// </summary>
        public Error? Bind(ASTCompiler compiler, string name, HIRExpression value)
            => Names(compiler) is { } names
                ? binder.Bind(names, name, value)
                : new ErrorMessage($"There are no names to define {name} in here.");

        public Error? Bind(Scope scope, string name, HIRExpression value)
            => binder.Bind(scope, name, value);

        private IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.IdentifierExpression identifier)
            => Names(compiler)?.Get(identifier.Name) is { } value
                ? Result<HIRExpression>.Ok(value)
                : Result<HIRExpression>.Error($"Name '{identifier.Name}' is not defined.");

        private IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.LiteralExpression literal)
            => literal.Type switch
            {
                AST.LiteralType.String => Result<HIRExpression>.Ok(new HIR.Code.StringLiteral(literal.Value)),
                AST.LiteralType.True => Result<HIRExpression>.Ok(new HIR.Code.BooleanLiteral(true)),
                AST.LiteralType.False => Result<HIRExpression>.Ok(new HIR.Code.BooleanLiteral(false)),
                _ => Result<HIRExpression>.Error($"{literal.Type} literals are not supported yet.")
            };

        private IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.CallExpression call)
        {
            if (compiler.CompileExpression(call.Callee).When(out var callee).Error(out var error))
                return Result<HIRExpression>.Error(error);

            var arguments = new List<HIR.Code.Argument>();

            foreach (var argument in call.Arguments)
                if (compiler.CompileExpression(argument.Value).When(out var value).Error(out error))
                    return Result<HIRExpression>.Error(error);
                else
                    arguments.Add(new() { Name = argument.Name, Value = value! });

            return Result<HIRExpression>.Ok(new HIR.Code.Call { Callee = callee!, Arguments = arguments });
        }

        private IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.BinaryExpression binary)
        {
            if (binary.Operator == "=")
                return Assign(compiler, binary);

            if (binary.Operator != ".")
                return Result<HIRExpression>.Error($"Operator ({binary.Operator}) is not supported yet.");

            if (binary.Right is not AST.IdentifierExpression member)
                return Result<HIRExpression>.Error("Only an identifier can follow the (.) operator.");

            if (compiler.CompileExpression(binary.Left).When(out var target).Error(out var error))
                return Result<HIRExpression>.Error(error);

            return Result<HIRExpression>.Ok(new HIR.Code.NamedMember { Target = target!, Name = member.Name });
        }

        private static IResult<HIRExpression, Error> Assign(ASTCompiler compiler, AST.BinaryExpression assignment)
        {
            if (compiler.CompileExpression(assignment.Left).When(out var target).Error(out var error))
                return Result<HIRExpression>.Error(error);

            return compiler.CompileExpression(assignment.Right).When(out var value).Error(out error)
                ? Result<HIRExpression>.Error(error)
                : Result<HIRExpression>.Ok(new HIR.Code.Set { Target = target!, Value = value! });
        }

        /// <summary>
        /// A <c>let</c> declares a local in the context the compiler is in.
        /// </summary>
        private IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.LetExpression let)
        {
            if (!compiler.Contexts.Has<IDeclaresLocals>(out var locals))
                return Result<HIRExpression>.Error($"Local {let.Name} isn't in a block.");

            if (let.Type is null && let.Value is null)
                return Result<HIRExpression>.Error($"Local {let.Name} needs a type or a value.");

            HIRExpression? type = null;
            HIRExpression? value = null;

            if (let.Type is not null && compiler.CompileExpression(let.Type).When(out type).Error(out var error))
                return Result<HIRExpression>.Error(error);

            if (let.Value is not null && compiler.CompileExpression(let.Value).When(out value).Error(out error))
                return Result<HIRExpression>.Error(error);

            var local = new HIR.Code.Local(let.Name, new HIR.Code.TypeExpression(type ?? new HIR.Code.TypeOf(value!)));

            locals.Declare(local);
            lets.Add(local);

            if (Bind(compiler, let.Name, new HIR.Code.Get(local)) is { } invalid)
                return Result<HIRExpression>.Error(invalid);

            return Result<HIRExpression>.Ok(value is null
                ? new HIR.Code.Get(local)
                : new HIR.Code.Set { Target = new HIR.Code.Get(local), Value = value }
            );
        }

        private static IResult<HIRExpression, Error> CompileDefinition(ASTCompiler compiler, AST.Definition definition)
            => compiler.CompileDefinition(definition).When(out var compiled).Error(out var error)
                ? Result<HIRExpression>.Error(error)
                : Build(compiler, compiled!, []);

        /// <summary>
        /// Takes the decorators off a statement, in source order.
        /// </summary>
        public static AST.Statement Undecorate(AST.Statement statement, out List<AST.Expression> decorators)
        {
            decorators = [];

            while (statement is AST.DecoratedStatement decorated)
            {
                decorators.Add(decorated.Decorator);
                statement = decorated.Statement;
            }

            return statement;
        }

        /// <summary>
        /// Compiles decorators with the given compiler.
        /// </summary>
        public static IResult<List<HIRExpression>, Error> CompileDecorators(ASTCompiler compiler, List<AST.Expression> decorators)
        {
            var compiled = new List<HIRExpression>();

            foreach (var decorator in decorators)
                if (compiler.CompileExpression(decorator).When(out var value).Error(out var error))
                    return Result<List<HIRExpression>>.Error(error);
                else
                    compiled.Add(value!);

            return Result<List<HIRExpression>>.Ok(compiled);
        }

        /// <summary>
        /// Calls the decorators on a value, the one nearest to it first, each on the
        /// result of the one before.
        /// </summary>
        public static HIRExpression Decorate(HIRExpression value, List<HIRExpression> decorators)
        {
            for (var i = decorators.Count - 1; i >= 0; i--)
                value = new HIR.Code.Call { Callee = decorators[i], Arguments = [new() { Value = value }] };

            return value;
        }

        /// <summary>
        /// A definition that isn't cast is an actual definition, so it is built by
        /// whatever builds the code it is in, which the context says.
        /// </summary>
        private static IResult<HIRExpression, Error> Build(ASTCompiler compiler, CompiledDefinition definition, List<HIRExpression> decorators)
            => compiler.Contexts.Has<IBuildsDefinitions>(out var builder)
                ? builder.Build(definition, decorators)
                : Result<HIRExpression>.Error($"Nothing builds the definition {definition.Name} here.");

        /// <summary>
        /// The one place that tells definitions apart: casting one to its underlying
        /// type keeps its build code, so the result is structure with no identity.
        /// Anything else is an ordinary cast.
        /// </summary>
        private IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.CastExpression cast)
        {
            if (compiler.CompileExpression(cast.TargetType).When(out var type).Error(out var error))
                return Result<HIRExpression>.Error(error);

            HIRExpression? value;

            if (cast.Expression is AST.Definition definition)
            {
                if (compiler.CompileDefinition(definition).When(out var compiled).Error(out error))
                    return Result<HIRExpression>.Error(error);

                if (isUnderlyingType(type!, compiled!.UnderlyingType))
                    return compiled.Build();

                if (Build(compiler, compiled, []).When(out value).Error(out error))
                    return Result<HIRExpression>.Error(error);
            }
            else if (compiler.CompileExpression(cast.Expression).When(out value).Error(out error))
                return Result<HIRExpression>.Error(error);

            return Result<HIRExpression>.Ok(new HIR.Code.Cast { Value = value!, Type = type! });
        }

        private static IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.ExpressionStatement statement)
            => compiler.CompileExpression(statement.Expression).When(out var expression).Error(out var error)
                ? Result<HIRStatement>.Error(error)
                : Result<HIRStatement>.Ok(new HIR.Code.ExpressionStatement(expression!));

        /// <summary>
        /// A definition statement also binds its name outside itself.
        /// </summary>
        private IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.DefinitionStatement statement)
            => Compile(compiler, statement, []);

        private IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.DecoratedStatement statement)
            => Undecorate(statement, out var decorators) is AST.DefinitionStatement definition
                ? Compile(compiler, definition, decorators)
                : Result<HIRStatement>.Error("Only a definition can be decorated so far.");

        private IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.DefinitionStatement statement, List<AST.Expression> decorators)
        {
            if (statement.Definition is not AST.Definition definition)
                return Result<HIRStatement>.Error("A definition statement needs a definition.");

            if (CompileDecorators(compiler, decorators).When(out var compiledDecorators).Error(out var error))
                return Result<HIRStatement>.Error(error);

            if (compiler.CompileDefinition(definition).When(out var compiled).Error(out error))
                return Result<HIRStatement>.Error(error);

            if (Build(compiler, compiled!, compiledDecorators!).When(out var value).Error(out error))
                return Result<HIRStatement>.Error(error);

            if (compiled!.Name is { Length: > 0 } name && Bind(compiler, name, value!) is { } invalid)
                return Result<HIRStatement>.Error(invalid);

            return Result<HIRStatement>.Ok(new HIR.Code.Block());
        }

        /// <summary>
        /// A block has its own names and declares its own locals, in its own context.
        /// </summary>
        private static IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.BlockStatement block)
        {
            var compiled = new HIR.Code.Block();
            var context = new BlockContext(compiler.Contexts.Current, new Scope(Names(compiler)), compiled);

            return compiler.Contexts.Within(context, () =>
            {
                foreach (var statement in block.Statements)
                    if (compiler.CompileStatement(statement).When(out var item).Error(out var error))
                        return Result<HIRStatement>.Error(error);
                    else
                        compiled.Statements.Add(item!);

                return Result<HIRStatement>.Ok(compiled);
            });
        }

        private static IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.Return statement)
        {
            if (statement.Value is null)
                return Result<HIRStatement>.Ok(new HIR.Code.Return());

            return compiler.CompileExpression(statement.Value).When(out var value).Error(out var error)
                ? Result<HIRStatement>.Error(error)
                : Result<HIRStatement>.Ok(new HIR.Code.Return { Value = value });
        }

        private static IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.IfStatement statement)
        {
            if (compiler.CompileExpression(statement.Condition).When(out var condition).Error(out var error))
                return Result<HIRStatement>.Error(error);

            if (compiler.CompileStatement(statement.If).When(out var then).Error(out error))
                return Result<HIRStatement>.Error(error);

            HIRStatement? otherwise = null;

            if (statement.Else is not null && compiler.CompileStatement(statement.Else).When(out otherwise).Error(out error))
                return Result<HIRStatement>.Error(error);

            return Result<HIRStatement>.Ok(new HIR.Code.IfStatement
            {
                Condition = condition!,
                Then = then!,
                Else = otherwise
            });
        }

        private IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.ImportStatement import)
        {
            if (compiler.CompileExpression(import.Source).When(out var source).Error(out var error))
                return Result<HIRStatement>.Error(error);

            var result = new HIR.Code.Reference();

            if (import.Alias is not null && Bind(compiler, import.Alias, new HIR.Code.Get(result)) is { } invalid)
                return Result<HIRStatement>.Error(invalid);

            foreach (var name in import.ImportedNames ?? [])
                if (Bind(
                    compiler,
                    name.Alias ?? name.Name,
                    new HIR.Code.NamedMember { Target = new HIR.Code.Get(result), Name = name.Name }
                ) is { } conflict)
                    return Result<HIRStatement>.Error(conflict);

            return Result<HIRStatement>.Ok(new Import(source!, result));
        }
    }
}
