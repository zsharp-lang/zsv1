namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RExpression Resolve(Expression expression)
            => expression switch
            {
                LiteralExpression literal => Resolve(literal),
                _ => NotImplemented<RExpression>(expression)
            };

        public static RLiteral Resolve(LiteralExpression literalExpression)
            => Utils.ParseLiteral(literalExpression);
    }
}
