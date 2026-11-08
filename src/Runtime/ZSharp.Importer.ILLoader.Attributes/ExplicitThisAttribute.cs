namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// The first parameter of a method or a constructor is the object it runs on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class ExplicitThisAttribute : Attribute
    {
    }
}
