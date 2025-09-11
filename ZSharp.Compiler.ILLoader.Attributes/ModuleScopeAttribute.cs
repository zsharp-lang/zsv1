namespace ZSharp.Compiler.ILLoader
{
    [AttributeUsage(
        AttributeTargets.Class, 
        AllowMultiple = false
    )]
    public sealed class ModuleScopeAttribute : Attribute
    {
    }
}
