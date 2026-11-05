namespace ZSharp.Compiler
{
    /// <summary>
    /// Implements <see cref="HIRCodeLoader"/> through registered loaders.
    /// </summary>
    public sealed class MappedCodeLoader(HIRCodeLoader loader)
    {
        private readonly HIRCodeLoader loader = loader;

        private HIRCodeLoader @base = null!;

        public LoaderMap<HIR.Code.Expression> Expressions { get; } = new();

        public LoaderMap<HIR.Code.Statement> Statements { get; } = new();

        public void Apply()
        {
            @base = loader.Clone();

            loader.LoadExpression = LoadExpression;
            loader.LoadStatement = LoadStatement;
        }

        public IResult LoadExpression(HIR.Code.Expression expression)
        {
            var result = @base.LoadExpression(expression);

            if (result.IsUnsupported())
                result = Expressions.Load(expression);

            return result;
        }

        public IResult LoadStatement(HIR.Code.Statement statement)
        {
            var result = @base.LoadStatement(statement);

            if (result.IsUnsupported())
                result = Statements.Load(statement);

            return result;
        }
    }
}
