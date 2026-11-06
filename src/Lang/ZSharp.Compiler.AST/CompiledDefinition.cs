namespace ZSharp.Compiler
{
    /// <summary>
    /// What a meta-compiler returns for a definition. It is not HIR and does not
    /// outlive whatever asked for it. How it was made is up to the meta-compiler.
    /// </summary>
    /// <param name="Variable">The local holding the definition object.</param>
    /// <param name="Construct">Code that constructs the object, empty.</param>
    /// <param name="UnderlyingType">The HIR type of that object.</param>
    /// <param name="Fill">
    /// Compiles the code that fills the object. It is called late, so that a container
    /// can have every name in scope first.
    /// </param>
    /// <param name="Name">The name a definition statement binds, if it has one.</param>
    public sealed record CompiledDefinition(
        HIR.Code.Local Variable,
        HIR.Code.Expression Construct,
        HIR.Instance UnderlyingType,
        Func<IResult<HIR.Code.Block, Error>> Fill,
        string? Name = null
    )
    {
        /// <summary>
        /// The whole definition as one expression that evaluates to the object.
        /// </summary>
        public IResult<HIR.Code.Expression, Error> Build()
        {
            if (Fill().When(out var fill).Error(out var error))
                return Result<HIR.Code.Expression>.Error(error);

            var build = new HIR.Code.BlockExpression
            {
                Result = new HIR.Code.Get(Variable),
                Statements = [
                    new HIR.Code.ExpressionStatement(new HIR.Code.Set
                    {
                        Target = new HIR.Code.Get(Variable),
                        Value = Construct
                    }),
                    fill!
                ]
            };

            build.Locals.Add(Variable);

            return Result<HIR.Code.Expression>.Ok(build);
        }
    }
}
