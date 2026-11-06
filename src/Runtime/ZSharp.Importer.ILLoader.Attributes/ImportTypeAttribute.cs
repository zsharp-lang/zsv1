namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Makes a type available as a member of this module, under its own name unless
    /// an alias is given.
    /// </summary>
    [AttributeUsage(AttributeTargets.Module, AllowMultiple = true)]
    public sealed class ImportTypeAttribute(Type type) : Attribute
    {
        public Type Type { get; } = type;

        public string? Alias { get; init; }
    }
}
