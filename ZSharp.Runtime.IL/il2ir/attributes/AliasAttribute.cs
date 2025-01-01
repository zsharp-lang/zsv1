namespace ZSharp.Runtime.NET.IL2IR
{
    [AttributeUsage(
        AttributeTargets.Module | 
        AttributeTargets.Class | 
        AttributeTargets.Struct | 
        AttributeTargets.Enum |
        AttributeTargets.Constructor |
        AttributeTargets.Method |
        AttributeTargets.Property,
        AllowMultiple = false)]
    public sealed class AliasAttribute : Attribute
    {
        public required string Name { get; set; }
    }
}
