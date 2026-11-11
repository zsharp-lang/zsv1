namespace Package.DotNet
{
    /// <summary>
    /// An existing object, as a value of the given type.
    /// </summary>
    public sealed class ExposedObject(object value, TypeReference type)
        : ZSharp.HIR.Instance
    {
        public object Value { get; set; } = value;

        public TypeReference Type { get; set; } = type;
    }
}
