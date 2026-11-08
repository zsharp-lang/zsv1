namespace ZSharp.Lang
{
    /// <summary>
    /// The flow of a definition that contains others, for meta-compilers that want it.
    /// Its code is plain code, running as part of the construction of the container.
    /// Each statement is emitted in order, and of a definition only its construction.
    /// Then every definition is filled, which is why they can refer to each other, and
    /// the container registers what it got.
    /// </summary>
    public sealed class ContainerCompiler(Language language)
    {
        /// <param name="compiler">The compiler the container is compiled with.</param>
        /// <param name="body">The statements of the container.</param>
        /// <param name="register">Code that adds a definition to the container.</param>
        /// <param name="member">
        /// Compiles a statement the container treats specially, returning
        /// <see langword="null"/> for one that is plain code.
        /// </param>
        public IResult<HIR.Code.Block, Error> Compile(
            ASTCompiler compiler,
            IEnumerable<AST.Statement> body,
            Func<CompiledDefinition, HIRExpression, HIRStatement> register,
            Func<AST.Statement, IResult<HIRStatement, Error>?>? member = null
        )
        {
            var block = new HIR.Code.Block();

            // A definition inside an expression is plain code here too, built in this block.
            return compiler.Contexts.Within(
                new BuildContext(compiler.Contexts.Current, block),
                () => Walk(compiler, block, body, register, member)
            );
        }

        private IResult<HIR.Code.Block, Error> Walk(
            ASTCompiler compiler,
            HIR.Code.Block block,
            IEnumerable<AST.Statement> body,
            Func<CompiledDefinition, HIRExpression, HIRStatement> register,
            Func<AST.Statement, IResult<HIRStatement, Error>?>? member
        )
        {
            var definitions = new List<(CompiledDefinition Definition, List<HIRExpression> Decorators)>();

            foreach (var statement in body)
            {
                if (Language.Undecorate(statement, out var decorators) is AST.DefinitionStatement { Definition: AST.Definition definition })
                {
                    if (Language.CompileDecorators(compiler, decorators).When(out var compiledDecorators).Error(out var error))
                        return Result<HIR.Code.Block>.Error(error);

                    if (compiler.CompileDefinition(definition).When(out var compiled).Error(out error))
                        return Result<HIR.Code.Block>.Error(error);

                    block.Locals.Add(compiled!.Variable);
                    block.Statements.Add(new HIR.Code.ExpressionStatement(new HIR.Code.Set
                    {
                        Target = new HIR.Code.Get(compiled.Variable),
                        Value = compiled.Construct
                    }));

                    if (compiled.Name is { Length: > 0 } name && language.Bind(compiler, name, new HIR.Code.Get(compiled.Variable)) is { } invalid)
                        return Result<HIR.Code.Block>.Error(invalid);

                    definitions.Add((compiled, compiledDecorators!));
                }
                else
                {
                    var result = member?.Invoke(statement) ?? compiler.CompileStatement(statement);

                    if (result.When(out var compiled).Error(out var error))
                        return Result<HIR.Code.Block>.Error(error);

                    block.Statements.Add(compiled!);
                }
            }

            foreach (var (definition, _) in definitions)
                if (definition.Fill().When(out var fill).Error(out var error))
                    return Result<HIR.Code.Block>.Error(error);
                else
                    block.Statements.Add(fill!);

            // A definition's decorators run right before it is registered.
            foreach (var (definition, decorators) in definitions)
                block.Statements.Add(register(definition, Language.Decorate(new HIR.Code.Get(definition.Variable), decorators)));

            return Result<HIR.Code.Block>.Ok(block);
        }
    }
}
