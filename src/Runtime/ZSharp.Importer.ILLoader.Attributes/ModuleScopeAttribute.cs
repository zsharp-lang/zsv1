namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Makes the static members of this class members of its module. C# has no
    /// module-level functions or variables, so a class stands in for that scope.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ModuleScopeAttribute : Attribute
    {
    }
}
