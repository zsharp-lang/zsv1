namespace ZSharp.Importer.ILLoader
{
    [AttributeUsage(
        AttributeTargets.Class, 
        AllowMultiple = false
    )]
    public sealed class ModuleScopeAttribute : Attribute
    {
    }
}
