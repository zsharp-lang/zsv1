namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// An object that exists in the running process, as a value of the given type. It is a
    /// runtime value rather than a definition of the CLR's, so it is the IL importer's: the
    /// runtime platform below it knows no HIR.
    /// </summary>
    public sealed class ExposedObject(object value, Package.DotNet.TypeReference type)
        : ZSharp.HIR.Instance
    {
        public object Value { get; set; } = value;

        public Package.DotNet.TypeReference Type { get; set; } = type;
    }
}
