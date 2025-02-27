namespace ZSharp.Objects
{
    public sealed class ArrayLiteral(IEnumerable<CompilerObject>? items = null)
        : CompilerObject
    {
        public List<CompilerObject> Items { get; } = new(items ?? []);
    }
}
