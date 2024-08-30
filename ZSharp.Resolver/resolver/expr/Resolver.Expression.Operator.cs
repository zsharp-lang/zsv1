namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RBinaryExpression Resolve(BinaryExpression binaryExpression)
            => new(
                Resolve(binaryExpression.Left),
                Resolve(binaryExpression.Right),
                binaryExpression.Operator
            );
    }
}
