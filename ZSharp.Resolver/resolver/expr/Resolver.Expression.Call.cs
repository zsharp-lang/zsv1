namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RCall Resolve(CallExpression callExpression)
            => new(
                Resolve(callExpression.Callee),
                callExpression.Arguments.Select(Resolve).ToArray()
            );

        public static RArgument Resolve(CallArgument argument)
            => new(argument.Name, Resolve(argument.Value));
    }
}
