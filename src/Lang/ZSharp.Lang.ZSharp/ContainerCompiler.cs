namespace ZSharp.Lang
{
    /// <summary>
    /// The flow of a definition that contains others, for meta-compilers that want it.
    /// Its code is plain code, running as part of the construction of the container.
    /// Each statement is emitted in order, and of a definition only its construction.
    /// Then every definition is filled, which is why they can refer to each other, and
    /// the container registers what it got.
    /// </summary>
    public sealed class ContainerCompiler(Language language, ASTCompiler compiler, MappedASTCompiler mapped)
    {
        /// <param name="body">The statements of the container.</param>
        /// <param name="register">Code that adds a definition to the container.</param>
        /// <param name="member">
        /// Compiles a statement the container treats specially, returning
        /// <see langword="null"/> for one that is plain code.
        /// </param>
        public IResult<HIR.Code.Block, Error> Compile(
            IEnumerable<AST.Statement> body,
            Func<CompiledDefinition, HIRStatement> register,
            Func<AST.Statement, IResult<HIRStatement, Error>?>? member = null
        )
        {
            var typeDefinitions = mapped.Expressions.Register<AST.TypeDefinition>(Plain);
            var modules = mapped.Expressions.Register<AST.Module>(Plain);
            var functions = mapped.Expressions.Register<AST.Function>(Plain);

            try
            {
                return Walk(body, register, member);
            }
            finally
            {
                mapped.Expressions.Register<AST.TypeDefinition>(typeDefinitions!);
                mapped.Expressions.Register<AST.Module>(modules!);
                mapped.Expressions.Register<AST.Function>(functions!);
            }
        }

        private IResult<HIR.Code.Block, Error> Walk(
            IEnumerable<AST.Statement> body,
            Func<CompiledDefinition, HIRStatement> register,
            Func<AST.Statement, IResult<HIRStatement, Error>?>? member
        )
        {
            var block = new HIR.Code.Block();
            var definitions = new List<CompiledDefinition>();

            foreach (var statement in body)
            {
                if (statement is AST.DefinitionStatement { Definition: AST.Definition definition })
                {
                    if (compiler.CompileDefinition(definition).When(out var compiled).Error(out var error))
                        return Result<HIR.Code.Block>.Error(error);

                    block.Locals.Add(compiled!.Variable);
                    block.Statements.Add(new HIR.Code.ExpressionStatement(new HIR.Code.Set
                    {
                        Target = new HIR.Code.Get(compiled.Variable),
                        Value = compiled.Construct
                    }));

                    if (compiled.Name is { Length: > 0 } name)
                        language.Scope.Add(name, new HIR.Code.Get(compiled.Variable));

                    definitions.Add(compiled);
                }
                else
                {
                    var result = member?.Invoke(statement) ?? compiler.CompileStatement(statement);

                    if (result.When(out var compiled).Error(out var error))
                        return Result<HIR.Code.Block>.Error(error);

                    block.Statements.Add(compiled!);
                }
            }

            foreach (var definition in definitions)
                if (definition.Fill().When(out var fill).Error(out var error))
                    return Result<HIR.Code.Block>.Error(error);
                else
                    block.Statements.Add(fill!);

            foreach (var definition in definitions)
                block.Statements.Add(register(definition));

            return Result<HIR.Code.Block>.Ok(block);
        }

        /// <summary>
        /// Code in a container is plain code, so a definition inside an expression just
        /// runs with it.
        /// </summary>
        private IResult<HIRExpression, Error> Plain(AST.Definition definition)
            => compiler.CompileDefinition(definition).When(out var compiled).Error(out var error)
                ? Result<HIRExpression>.Error(error)
                : compiled!.Build();
    }
}
