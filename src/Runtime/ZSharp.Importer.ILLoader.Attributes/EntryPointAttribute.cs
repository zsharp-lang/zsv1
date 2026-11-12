namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Marks a static method with no parameters as an entry point of its module: what
    /// loads it into the environment that imports it, called once, when it is imported.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class EntryPointAttribute : Attribute
    {
    }
}
