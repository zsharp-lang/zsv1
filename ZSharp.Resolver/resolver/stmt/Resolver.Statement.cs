namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RStatement Resolve(Statement statement)
            => statement switch
            {
                BlockStatement block => Resolve(block),
                ExpressionStatement expressionStatement => Resolve(expressionStatement),
                ImportStatement importStatement => Resolve(importStatement),

                Return @return => Resolve(@return),

                _ => NotImplemented<RStatement>(statement)
            };

        public static RExpressionStatement Resolve(ExpressionStatement expressionStatement)
            => new(Resolve(expressionStatement.Expression));

        public static RBlock Resolve(BlockStatement block)
            => new(new(block.Statements.Select(Resolve)));
    }
}
