namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RExpression Resolve(Expression expression)
            => expression switch
            {
                BinaryExpression binary => Resolve(binary),
                CallExpression call => Resolve(call),
                Function function => Resolve(function),
                IdentifierExpression identifier => Resolve(identifier),
                LetExpression let => Resolve(let),
                LiteralExpression literal => Resolve(literal),
                Module module => Resolve(module),
                _ => NotImplemented<RExpression>(expression)
            };

        public static RId Resolve(IdentifierExpression identifierExpression)
            => new(identifierExpression.Name);

        public static RLiteral Resolve(LiteralExpression literalExpression)
            => Utils.ParseLiteral(literalExpression);
    }
}
