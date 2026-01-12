namespace ZSharp.Importer.ILLoader
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Constructor |
        AttributeTargets.Enum |
        AttributeTargets.Field |
        AttributeTargets.GenericParameter |
        AttributeTargets.Interface |
        AttributeTargets.Method |
        AttributeTargets.Module |
        AttributeTargets.Parameter |
        AttributeTargets.Property |
        AttributeTargets.Struct,
        AllowMultiple = false
    )]
    public sealed class AliasAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}
