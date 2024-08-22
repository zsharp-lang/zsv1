namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RStatement Resolve(Statement statement)
            => statement switch
            {
                ExpressionStatement expressionStatement => Resolve(expressionStatement),
                ImportStatement importStatement => Resolve(importStatement),
                _ => NotImplemented<RStatement>(statement)
            };

        public static RExpressionStatement Resolve(ExpressionStatement expressionStatement)
            => new(Resolve(expressionStatement.Expression));
    }
}
