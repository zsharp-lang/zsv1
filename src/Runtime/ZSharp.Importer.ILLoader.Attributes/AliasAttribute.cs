namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// The name a member is known by in Z#.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class AliasAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}
