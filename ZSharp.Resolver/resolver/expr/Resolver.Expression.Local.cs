namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RLetDefinition Resolve(LetExpression letExpression)
            => new(
                letExpression.Name,
                letExpression.Type is null ? null : Resolve(letExpression.Type),
                Resolve(letExpression.Value)
            );
    }
}
