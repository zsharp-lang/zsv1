namespace ZSharp.Compiler.ILLoader
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Interface |
        AttributeTargets.Enum,
        AllowMultiple = false,
        Inherited = false
    )]
    public sealed class AddNamespaceAttribute(string @namespace) : Attribute
    {
        public string Name { get; } = @namespace;
        
        public bool IsGlobalNamespace => string.IsNullOrEmpty(Name);
    }
}
