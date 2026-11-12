namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// An annotation kept on what a definition was built as, as CLR platforms write one:
    /// its type, and each of its fields by name, whose values are constants.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class MetadataAttribute(Type type, string[] names, object[] values) : Attribute
    {
        public Type Type { get; } = type;

        public string[] Names { get; } = names;

        public object[] Values { get; } = values;
    }
}
