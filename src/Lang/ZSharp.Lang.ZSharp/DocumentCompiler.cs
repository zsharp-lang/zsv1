namespace ZSharp.Lang
{
    /// <summary>
    /// Compiles a document, which runs statement by statement. Nothing builds the code of
    /// a document, so the definitions in a statement are built, loaded and run right
    /// before it.
    /// </summary>
    public sealed class DocumentCompiler
    {
        /// <summary>
        /// The names of the document.
        /// </summary>
        public Scope Scope { get; } = new();

        public IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.Statement statement)
        {
            var context = new DocumentContext(Scope);

            var result = compiler.Contexts.Within(context, () => compiler.CompileStatement(statement));

            if (context.Defined.Count == 0 || result.When(out var compiled).Error(out _))
                return result;

            var block = new HIR.Code.Block();

            block.Statements.AddRange(context.Defined);
            block.Statements.Add(compiled!);

            return Result<HIRStatement>.Ok(block);
        }

        /// <summary>
        /// Compiles something as if it were in the document.
        /// </summary>
        public T Within<T>(ASTCompiler compiler, Func<T> compile)
            => compiler.Contexts.Within(new DocumentContext(Scope), compile);

        private sealed class DocumentContext(Scope names)
            : Context
            , INames
            , IBuildsDefinitions
        {
            public Scope Names { get; } = names;

            public List<HIRStatement> Defined { get; } = [];

            public IResult<HIRExpression, Error> Build(CompiledDefinition definition, List<HIRExpression> decorators)
            {
                if (definition.Build().When(out var build).Error(out var error))
                    return Result<HIRExpression>.Error(error);

                var result = new HIR.Code.Reference();

                Defined.Add(new Define(Language.Decorate(build!, decorators), result));

                return Result<HIRExpression>.Ok(new HIR.Code.Get(result));
            }
        }
    }
}
