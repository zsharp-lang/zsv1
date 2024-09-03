namespace ZSharp.CGObjects
{
    public sealed class ImportedName : CGObject
    {
        public required string Name { get; set; }

        public string? Alias { get; set; }
    }
}
