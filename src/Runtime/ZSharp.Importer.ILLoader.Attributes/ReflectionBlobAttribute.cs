namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Says the assembly carries what an encoder wrote of its modules, as the resource of
    /// this name, and what decodes it: the source of the library to import, as
    /// <c>std:encoding</c>. The CLR's convention for where a reflection blob is.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class ReflectionBlobAttribute(string resource, string? decoder) : Attribute
    {
        public ReflectionBlobAttribute(string resource) : this(resource, null) { }

        public string Resource { get; } = resource;

        public string? Decoder { get; } = decoder;
    }
}
