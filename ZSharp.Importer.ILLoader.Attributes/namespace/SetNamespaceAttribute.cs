namespace ZSharp.Importer.ILLoader
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Interface |
        AttributeTargets.Enum,
        AllowMultiple = false,
        Inherited = false
    )]
    public sealed class SetNamespaceAttribute(string @namespace) : Attribute
    {
        public string Name { get; } = @namespace;
        
        public bool IsGlobalNamespace => string.IsNullOrEmpty(Name);
    }
}
