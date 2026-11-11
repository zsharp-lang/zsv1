namespace ZSharp.Lang
{
    /// <summary>
    /// The built-in Z# language. It is an extension like any other: everything it does
    /// is registered here. It keeps no context of its own: each handler compiles with the
    /// compiler it runs in, and asks that compiler's contexts for what it needs.
    /// </summary>
    public sealed partial class Language(Binder binder)
    {
        private Func<HIRExpression, HIR.Instance, bool> isUnderlyingType = null!;

        /// <summary>
        /// The functions that syntax compiles to calls of: an operator, such as <c>+</c>,
        /// and <c>import</c>. Each is an overload group, so more overloads can be added.
        /// </summary>
        public Dictionary<string, HIR.Code.OverloadGroup> Functions { get; } = [];

        /// <summary>
        /// Adds an overload to the function a piece of syntax stands for.
        /// </summary>
        public void Add(string syntax, HIR.Instance function)
        {
            if (!Functions.TryGetValue(syntax, out var group))
                Functions[syntax] = group = new();

            group.Items.Add(function);
        }

        /// <param name="isUnderlyingType">
        /// Whether code that evaluates to a type is the given definition type.
        /// </param>
        public void Install(
            MappedASTCompiler mapped,
            Func<HIRExpression, HIR.Instance, bool> isUnderlyingType
        )
        {
            this.isUnderlyingType = isUnderlyingType;

            mapped.Expressions.Register<AST.IdentifierExpression>(Compile);
            mapped.Expressions.Register<AST.LiteralExpression>(Compile);
            mapped.Expressions.Register<AST.CallExpression>(Compile);
            mapped.Expressions.Register<AST.IndexExpression>(Compile);
            mapped.Expressions.Register<AST.BinaryExpression>(Compile);
            mapped.Expressions.Register<AST.UnaryExpression>(Compile);
            mapped.Expressions.Register<AST.CastExpression>(Compile);
            mapped.Expressions.Register<AST.IsOfExpression>(Compile);
            mapped.Expressions.Register<AST.ArrayLiteral>(Compile);
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

            InstallLoops(mapped);
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
                AST.LiteralType.Null => Result<HIRExpression>.Ok(new HIR.Code.NullLiteral()),
                AST.LiteralType.Number => int.TryParse(literal.Value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var integer)
                    ? Result<HIRExpression>.Ok(new HIR.Code.IntegerLiteral(integer))
                    : Result<HIRExpression>.Error($"{literal.Value} is too large for an Int32."),
                AST.LiteralType.Decimal => double.TryParse(literal.Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var number)
                    ? Result<HIRExpression>.Ok(new HIR.Code.DecimalLiteral(number))
                    : Result<HIRExpression>.Error($"{literal.Value} isn't a number."),
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

        private IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.IndexExpression index)
        {
            if (compiler.CompileExpression(index.Target).When(out var target).Error(out var error))
                return Result<HIRExpression>.Error(error);

            var arguments = new List<HIR.Code.Argument>();

            foreach (var argument in index.Arguments)
                if (compiler.CompileExpression(argument.Value).When(out var value).Error(out error))
                    return Result<HIRExpression>.Error(error);
                else
                    arguments.Add(new() { Name = argument.Name, Value = value! });

            return Result<HIRExpression>.Ok(new HIR.Code.Index { Target = target!, Arguments = arguments });
        }

        private IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.BinaryExpression binary)
        {
            if (binary.Operator == "=")
                return Assign(compiler, binary);

            if (binary.Operator != ".")
                return Operator(compiler, binary);

            if (binary.Right is not AST.IdentifierExpression member)
                return Result<HIRExpression>.Error("Only an identifier can follow the (.) operator.");

            if (compiler.CompileExpression(binary.Left).When(out var target).Error(out var error))
                return Result<HIRExpression>.Error(error);

            return Result<HIRExpression>.Ok(new HIR.Code.NamedMember { Target = target!, Name = member.Name });
        }

        /// <summary>
        /// An operator is a call of the function it stands for, with both operands.
        /// </summary>
        private IResult<HIRExpression, Error> Operator(ASTCompiler compiler, AST.BinaryExpression binary)
        {
            if (!Functions.TryGetValue(binary.Operator, out var function))
                return Result<HIRExpression>.Error($"Operator ({binary.Operator}) is not defined.");

            if (compiler.CompileExpression(binary.Left).When(out var left).Error(out var error))
                return Result<HIRExpression>.Error(error);

            if (compiler.CompileExpression(binary.Right).When(out var right).Error(out error))
                return Result<HIRExpression>.Error(error);

            return Result<HIRExpression>.Ok(new HIR.Code.Call
            {
                Callee = new HIR.Code.Get(function),
                Arguments = [new() { Value = left! }, new() { Value = right! }]
            });
        }

        /// <summary>
        /// An operator before its operand is a call of the function it stands for, with the
        /// operand alone: an overload of it that takes one, as `-` has for each number.
        /// </summary>
        private IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.UnaryExpression unary)
        {
            if (!Functions.TryGetValue(unary.Operator, out var function))
                return Result<HIRExpression>.Error($"Operator ({unary.Operator}) is not defined.");

            if (compiler.CompileExpression(unary.Operand).When(out var operand).Error(out var error))
                return Result<HIRExpression>.Error(error);

            return Result<HIRExpression>.Ok(new HIR.Code.Call
            {
                Callee = new HIR.Code.Get(function),
                Arguments = [new() { Value = operand! }]
            });
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
        /// A <c>let</c> doesn't choose its meta-type: whatever contains it compiles it in
        /// its own terms, through its fork of the compiler.
        /// </summary>
        private static IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.LetExpression let)
            => Result<HIRExpression>.Error($"Nothing here compiles the let {let.Name}.");

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

        private static IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.ArrayLiteral array)
        {
            var literal = new HIR.Code.ArrayLiteral();

            foreach (var item in array.Items)
                if (compiler.CompileExpression(item).When(out var value).Error(out var error))
                    return Result<HIRExpression>.Error(error);
                else
                    literal.Items.Add(value!);

            return Result<HIRExpression>.Ok(literal);
        }

        /// <summary>
        /// <c>value is of T</c> tests whether the value is of a type. The only pattern so
        /// far is a name, as in <c>value is name of T</c>, which also defines a variable of
        /// the type and assigns it what matched, cast to the type. <c>_</c> names nothing.
        /// </summary>
        private static IResult<HIRExpression, Error> Compile(ASTCompiler compiler, AST.IsOfExpression match)
        {
            if (compiler.CompileExpression(match.Expression).When(out var value).Error(out var error))
                return Result<HIRExpression>.Error(error);

            if (compiler.CompileExpression(match.OfType).When(out var type).Error(out error))
                return Result<HIRExpression>.Error(error);

            if (match.Name is not { Length: > 0 } name || name == "_")
                return Result<HIRExpression>.Ok(new HIR.Code.Match { Value = value!, Pattern = type! });

            return Named(compiler, name, match.OfType, value!, type!);
        }

        /// <summary>
        /// A name pattern is a local like any other, defined where the match is by whatever
        /// defines locals there, so it can be read and assigned after it. It is assigned
        /// only if the value matched. The value is evaluated once.
        /// </summary>
        private static IResult<HIRExpression, Error> Named(ASTCompiler compiler, string name, AST.Expression typeSyntax, HIRExpression value, HIRExpression type)
        {
            if (compiler.CompileExpression(new AST.LetExpression { Name = name, Type = typeSyntax }).When(out var defined).Error(out var error))
                return Result<HIRExpression>.Error(error);

            if (Names(compiler)?.Get(name) is not { } variable)
                return Result<HIRExpression>.Error($"The pattern {name} didn't define a name.");

            var matched = new HIR.Code.Local(string.Empty, HIR.Infer.Value) { Init = value };
            var result = new HIR.Code.Local(string.Empty, HIR.Infer.Value)
            {
                Init = new HIR.Code.Match { Value = new HIR.Code.Get(matched), Pattern = type }
            };

            return Result<HIRExpression>.Ok(new HIR.Code.BlockExpression
            {
                Statements =
                [
                    new HIR.Code.ExpressionStatement(defined!),
                    new HIR.Code.Declare(matched),
                    new HIR.Code.Declare(result),
                    new HIR.Code.IfStatement
                    {
                        Condition = new HIR.Code.Get(result),
                        Then = new HIR.Code.ExpressionStatement(new HIR.Code.Set
                        {
                            Target = variable,
                            Value = new HIR.Code.Cast { Value = new HIR.Code.Get(matched), Type = type }
                        })
                    }
                ],
                Result = new HIR.Code.Get(result)
            });
        }

        private static IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.ExpressionStatement statement)
            => statement.Expression is AST.WhileExpression<AST.Statement> loop
                ? Loop(compiler, loop)
                : compiler.CompileExpression(statement.Expression).When(out var expression).Error(out var error)
                ? Result<HIRStatement>.Error(error)
                : Result<HIRStatement>.Ok(new HIR.Code.ExpressionStatement(expression!));

        /// <summary>
        /// A definition statement also binds its name outside itself.
        /// </summary>
        private IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.DefinitionStatement statement)
            => Compile(compiler, statement, []);

        /// <summary>
        /// A decorated statement that isn't a definition is, in plain code, a call of its
        /// decorators on the statement as HIR, made as the code runs. What they give is
        /// dropped. A meta-compiler whose code is built, as a body is, places it instead.
        /// </summary>
        private IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.DecoratedStatement statement)
        {
            if (Undecorate(statement, out var decorators) is AST.DefinitionStatement definition)
                return Compile(compiler, definition, decorators);

            return Decorated(compiler, Undecorate(statement, out _), decorators).When(out var call).Error(out var error)
                ? Result<HIRStatement>.Error(error)
                : Result<HIRStatement>.Ok(new HIR.Code.ExpressionStatement(call!));
        }

        /// <summary>
        /// The call of decorators on a statement, which they are given as HIR: the code
        /// that builds the statement's node. An expression statement gives them its
        /// expression, so <c>@ct print("x")</c> calls <c>ct</c> with the call of <c>print</c>.
        /// </summary>
        public static IResult<HIRExpression, Error> Decorated(ASTCompiler compiler, AST.Statement statement, List<AST.Expression> decorators)
        {
            if (CompileDecorators(compiler, decorators).When(out var compiledDecorators).Error(out var error))
                return Result<HIRExpression>.Error(error);

            IResult<HIRExpression, Error> reified;

            if (statement is AST.ExpressionStatement { Expression: var expression })
                reified = compiler.CompileExpression(expression).When(out var compiled).Error(out error)
                    ? Result<HIRExpression>.Error(error)
                    : ZSharp.Compiler.Current.HIRReifier.ReifyExpression(compiled!);
            else
                reified = compiler.CompileStatement(statement).When(out var compiled).Error(out error)
                    ? Result<HIRExpression>.Error(error)
                    : ZSharp.Compiler.Current.HIRReifier.ReifyStatement(compiled!);

            return reified.When(out var node).Error(out error)
                ? Result<HIRExpression>.Error(error)
                : Result<HIRExpression>.Ok(Decorate(node!, compiledDecorators!));
        }

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
            var context = new BlockContext(compiler.Contexts.Current, new Scope(Names(compiler)));

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

        /// <summary>
        /// An import means what the meta-compiler it is in makes it mean: where it runs,
        /// and what holds what it imports.
        /// </summary>
        private IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.ImportStatement import)
            => Result<HIRStatement>.Error("Nothing here compiles the import.");

        /// <summary>
        /// The code an import runs: a call of the <c>import</c> function with the source,
        /// then the import's own arguments, as in <c>import(args...) { names } from source</c>.
        /// Where it runs, and what holds what it gives, is up to the meta-compiler.
        /// </summary>
        public IResult<HIRExpression, Error> Importing(ASTCompiler compiler, AST.ImportStatement import)
        {
            if (!Functions.TryGetValue("import", out var function))
                return Result<HIRExpression>.Error("Nothing imports here.");

            if (compiler.CompileExpression(import.Source).When(out var source).Error(out var error))
                return Result<HIRExpression>.Error(error);

            var arguments = new List<HIR.Code.Argument> { new() { Value = source! } };

            foreach (var argument in import.Arguments ?? [])
                if (compiler.CompileExpression(argument.Value).When(out var value).Error(out error))
                    return Result<HIRExpression>.Error(error);
                else
                    arguments.Add(new() { Name = argument.Name, Value = value! });

            return Result<HIRExpression>.Ok(new HIR.Code.Call { Callee = new HIR.Code.Get(function), Arguments = arguments });
        }

        /// <summary>
        /// Binds the names an import brings in to what holds the imported value, for
        /// meta-compilers that compile imports.
        /// </summary>
        /// <param name="imported">Code that gives the imported value, made once for each name.</param>
        /// <summary>
        /// Binds the names of an import that ran while compiling to the members of what it
        /// gave. Each member is reached as the import runs, as importing it means, so what
        /// comes with it, as a class's extension methods, comes then, and a name the
        /// imported value has no member for is an error of the import.
        /// </summary>
        public Error? BindImported(ASTCompiler compiler, AST.ImportStatement import, HIR.Instance imported)
        {
            foreach (var name in import.ImportedNames ?? [])
                if (ZSharp.Compiler.Current.HIRCodeLoader
                    .LoadExpression(new HIR.Code.NamedMember { Target = new HIR.Code.Get(imported), Name = name.Name })
                    .Error(out var error))
                    return new ErrorMessage($"The import of {name.Name} failed: {error}");

            return BindImported(compiler, import, () => new HIR.Code.Get(imported));
        }

        public Error? BindImported(ASTCompiler compiler, AST.ImportStatement import, Func<HIRExpression> imported)
        {
            foreach (var name in import.ImportedNames ?? [])
                if (Bind(
                    compiler,
                    name.Alias ?? name.Name,
                    new HIR.Code.NamedMember { Target = imported(), Name = name.Name }
                ) is { } conflict)
                    return conflict;

            return null;
        }
    }
}
