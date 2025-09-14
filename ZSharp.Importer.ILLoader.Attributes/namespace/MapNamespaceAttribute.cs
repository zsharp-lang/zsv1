namespace ZSharp.Importer.ILLoader
{
    [AttributeUsage(
        AttributeTargets.Module,
        AllowMultiple = true
    )]
    public sealed class MapNamespaceAttribute : Attribute
    {
        public required string OldName { get; init; }

        public required string NewName { get; init; }
    }
}
