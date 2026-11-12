namespace ZSharp.Compiler
{
    public delegate IResult LoadExpression(HIR.Code.Expression expression);

    public delegate IResult LoadStatement(HIR.Code.Statement statement);

    public class HIRCodeLoader
    {
        public LoadExpression LoadExpression { get; set; } = expression => Unsupported("expression", expression);

        public LoadStatement LoadStatement { get; set; } = statement => Unsupported("statement", statement);

        public HIRCodeLoader Clone() => (HIRCodeLoader)MemberwiseClone();

        private static IResult Unsupported(string kind, object node)
            => Result.Error(new UnsupportedError($"Can't load HIR {kind} of type {node.GetType().Name}."));
    }
}
