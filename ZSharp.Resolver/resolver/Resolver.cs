namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static IEnumerable<RStatement> Resolve(Document document)
            => document.Statements.Select(Resolve);

        private static T NotImplemented<T>(Node node)
            => throw new NotImplementedException($"Resolver({node.GetType().Name}) is not implemented");
    }
}
