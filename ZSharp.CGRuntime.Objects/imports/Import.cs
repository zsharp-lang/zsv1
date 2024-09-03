namespace ZSharp.CGObjects
{
    public sealed class Import : CGObject
    {
        public required CGCode Arguments { get; set; }

        public required List<ImportedName> ImportedNames { get; set; }

        public string? Alias { get; set; }
    }
}
