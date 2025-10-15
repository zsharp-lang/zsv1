namespace ZSharp.Importer.ILLoader
{
    [AttributeUsage(
        AttributeTargets.Module | AttributeTargets.Class,
        AllowMultiple = true
    )]
    public sealed class ImportTypeAttribute(Type type) : Attribute
    {
        public Type Type { get; } = type;

        public string? Alias { get; init; } = null;

        public string? Namespace { get; init; } = null;
    }
}
