namespace ZSharp.Lang
{
    /// <summary>
    /// The built-in Z# language. It is an extension like any other: everything it does
    /// is registered here.
    /// </summary>
    public sealed class Language(ASTCompiler compiler)
    {
        private readonly ASTCompiler compiler = compiler;

        private Func<HIRExpression, HIR.Instance, bool> isUnderlyingType = null!;

        public Scope Scope { get; set; } = new();

        /// <param name="isUnderlyingType">
        /// Whether code that evaluates to a type is the given definition type.
        /// </param>
        public void Install(MappedASTCompiler mapped, Func<HIRExpression, HIR.Instance, bool> isUnderlyingType)
        {
            this.isUnderlyingType = isUnderlyingType;

            mapped.Expressions.Register<AST.IdentifierExpression>(Compile);
            mapped.Expressions.Register<AST.LiteralExpression>(Compile);
            mapped.Expressions.Register<AST.CallExpression>(Compile);
            mapped.Expressions.Register<AST.BinaryExpression>(Compile);
            mapped.Expressions.Register<AST.CastExpression>(Compile);
            mapped.Expressions.Register<AST.Function>(CompileDefinition);
            mapped.Expressions.Register<AST.TypeDefinition>(CompileDefinition);
            mapped.Expressions.Register<AST.Module>(CompileDefinition);

            mapped.Statements.Register<AST.ExpressionStatement>(Compile);
            mapped.Statements.Register<AST.DefinitionStatement>(Compile);
            mapped.Statements.Register<AST.BlockStatement>(Compile);
            mapped.Statements.Register<AST.IfStatement>(Compile);
            mapped.Statements.Register<AST.ImportStatement>(Compile);
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

                return load(instance);
            });
        }

        private IResult<HIRExpression, Error> Compile(AST.IdentifierExpression identifier)
            => Scope.Get(identifier.Name) is { } value
                ? Result<HIRExpression>.Ok(value)
                : Result<HIRExpression>.Error($"Name '{identifier.Name}' is not defined.");

        private IResult<HIRExpression, Error> Compile(AST.LiteralExpression literal)
            => literal.Type switch
            {
                AST.LiteralType.String => Result<HIRExpression>.Ok(new HIR.Code.StringLiteral(literal.Value)),
                AST.LiteralType.True => Result<HIRExpression>.Ok(new HIR.Code.BooleanLiteral(true)),
                AST.LiteralType.False => Result<HIRExpression>.Ok(new HIR.Code.BooleanLiteral(false)),
                _ => Result<HIRExpression>.Error($"{literal.Type} literals are not supported yet.")
            };

        private IResult<HIRExpression, Error> Compile(AST.CallExpression call)
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

        private IResult<HIRExpression, Error> Compile(AST.BinaryExpression binary)
        {
            if (binary.Operator != ".")
                return Result<HIRExpression>.Error($"Operator ({binary.Operator}) is not supported yet.");

            if (binary.Right is not AST.IdentifierExpression member)
                return Result<HIRExpression>.Error("Only an identifier can follow the (.) operator.");

            if (compiler.CompileExpression(binary.Left).When(out var target).Error(out var error))
                return Result<HIRExpression>.Error(error);

            return Result<HIRExpression>.Ok(new HIR.Code.NamedMember { Target = target!, Name = member.Name });
        }

        /// <summary>
        /// A definition that isn't cast is an actual definition, so it runs with
        /// whatever builds the code it is in, not with that code.
        /// </summary>
        private IResult<HIRExpression, Error> CompileDefinition(AST.Definition definition)
        {
            if (compiler.CompileDefinition(definition).When(out var compiled).Error(out var error))
                return Result<HIRExpression>.Error(error);

            return compiled!.Build().When(out var build).Error(out error)
                ? Result<HIRExpression>.Error(error)
                : Result<HIRExpression>.Ok(new HIR.Code.Escape(build!));
        }

        /// <summary>
        /// The one place that tells definitions apart: casting one to its underlying
        /// type keeps its build code, so the result is structure with no identity.
        /// Anything else is an ordinary cast. A meta-compiler overrides this to
        /// decide where that build code goes.
        /// </summary>
        private IResult<HIRExpression, Error> Compile(AST.CastExpression cast)
        {
            if (compiler.CompileExpression(cast.TargetType).When(out var type).Error(out var error))
                return Result<HIRExpression>.Error(error);

            HIRExpression? value;

            if (cast.Expression is AST.Definition definition)
            {
                if (compiler.CompileDefinition(definition).When(out var compiled).Error(out error))
                    return Result<HIRExpression>.Error(error);

                if (compiled!.Build().When(out var build).Error(out error))
                    return Result<HIRExpression>.Error(error);

                if (isUnderlyingType(type!, compiled.UnderlyingType))
                    return Result<HIRExpression>.Ok(build!);

                value = new HIR.Code.Escape(build!);
            }
            else if (compiler.CompileExpression(cast.Expression).When(out value).Error(out error))
                return Result<HIRExpression>.Error(error);

            return Result<HIRExpression>.Ok(new HIR.Code.Cast { Value = value!, Type = type! });
        }

        private IResult<HIRStatement, Error> Compile(AST.ExpressionStatement statement)
            => compiler.CompileExpression(statement.Expression).When(out var expression).Error(out var error)
                ? Result<HIRStatement>.Error(error)
                : Result<HIRStatement>.Ok(new HIR.Code.ExpressionStatement(expression!));

        /// <summary>
        /// A definition statement also binds its name outside itself.
        /// </summary>
        private IResult<HIRStatement, Error> Compile(AST.DefinitionStatement statement)
        {
            if (statement.Definition is not AST.Definition definition)
                return Result<HIRStatement>.Error("A definition statement needs a definition.");

            if (compiler.CompileDefinition(definition).When(out var compiled).Error(out var error))
                return Result<HIRStatement>.Error(error);

            if (compiled!.Build().When(out var build).Error(out error))
                return Result<HIRStatement>.Error(error);

            var result = new HIR.Code.Reference();

            if (compiled.Name is { Length: > 0 } name)
                Scope.Add(name, new HIR.Code.Get(result));

            return Result<HIRStatement>.Ok(new Define(build!, result));
        }

        private IResult<HIRStatement, Error> Compile(AST.BlockStatement block)
        {
            var previous = Scope;
            Scope = new(previous);

            try
            {
                var statements = new List<HIRStatement>();

                foreach (var statement in block.Statements)
                    if (compiler.CompileStatement(statement).When(out var compiled).Error(out var error))
                        return Result<HIRStatement>.Error(error);
                    else
                        statements.Add(compiled!);

                return Result<HIRStatement>.Ok(new HIR.Code.Block(statements));
            }
            finally
            {
                Scope = previous;
            }
        }

        private IResult<HIRStatement, Error> Compile(AST.IfStatement statement)
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

        private IResult<HIRStatement, Error> Compile(AST.ImportStatement import)
        {
            if (compiler.CompileExpression(import.Source).When(out var source).Error(out var error))
                return Result<HIRStatement>.Error(error);

            var result = new HIR.Code.Reference();

            if (import.Alias is not null)
                Scope.Add(import.Alias, new HIR.Code.Get(result));

            foreach (var name in import.ImportedNames ?? [])
                Scope.Add(
                    name.Alias ?? name.Name,
                    new HIR.Code.NamedMember { Target = new HIR.Code.Get(result), Name = name.Name }
                );

            return Result<HIRStatement>.Ok(new Import(source!, result));
        }
    }
}
