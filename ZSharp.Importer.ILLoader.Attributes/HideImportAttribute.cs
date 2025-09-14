namespace ZSharp.Importer.ILLoader
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Constructor |
        AttributeTargets.Enum |
        AttributeTargets.Field |
        AttributeTargets.Interface |
        AttributeTargets.Method |
        AttributeTargets.Property |
        AttributeTargets.Struct,
        AllowMultiple = false
    )]
    public sealed class HideImportAttribute : Attribute
    {
        
    }
}
