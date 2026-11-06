namespace Package.DotNet
{
    /// <summary>
    /// An existing object, as a value of the given class.
    /// </summary>
    public sealed class ExposedObject(object value, Class type)
        : ZSharp.HIR.Instance
    {
        public object Value { get; set; } = value;

        public Class Type { get; set; } = type;
    }
}
