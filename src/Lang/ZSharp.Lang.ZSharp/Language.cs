namespace ZSharp.Lang
{
    /// <summary>
    /// The built-in Z# language. It is an extension like any other: everything it does
    /// is registered here.
    /// </summary>
    public sealed class Language(ASTCompiler compiler)
    {
        private readonly ASTCompiler compiler = compiler;

        public Scope Scope { get; private set; } = new();

        public void Install(MappedASTCompiler mapped)
        {
            mapped.Expressions.Register<AST.IdentifierExpression>(Compile);
            mapped.Expressions.Register<AST.LiteralExpression>(Compile);
            mapped.Expressions.Register<AST.CallExpression>(Compile);
            mapped.Expressions.Register<AST.BinaryExpression>(Compile);

            mapped.Statements.Register<AST.ExpressionStatement>(Compile);
            mapped.Statements.Register<AST.BlockStatement>(Compile);
            mapped.Statements.Register<AST.IfStatement>(Compile);
            mapped.Statements.Register<AST.ImportStatement>(Compile);
        }

        /// <param name="import">
        /// Imports from the value an import source evaluates to. A source is an
        /// expression, so that value can be anything.
        /// </param>
        public static void Install(
            MappedRunner mapped,
            HIRRunner runner,
            Func<object?, IResult<HIR.Instance, Error>> import
        )
            => mapped.Register<Import>(statement =>
            {
                if (runner.EvaluateExpression(statement.Source).When(out var source).Error(out var error))
                    return error;

                if (import(source).When(out var result).Error(out error))
                    return error;

                statement.Result.Value = result;

                return null;
            });

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

        private IResult<HIRStatement, Error> Compile(AST.ExpressionStatement statement)
            => compiler.CompileExpression(statement.Expression).When(out var expression).Error(out var error)
                ? Result<HIRStatement>.Error(error)
                : Result<HIRStatement>.Ok(new HIR.Code.ExpressionStatement(expression!));

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
