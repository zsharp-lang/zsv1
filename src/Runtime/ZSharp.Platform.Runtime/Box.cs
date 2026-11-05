namespace ZSharp.Platform.Runtime
{
    /// <summary>
    /// What a <see cref="LIR.Reference"/> points at. The CLR has no first-class
    /// reference to a value that can be stored in a field.
    /// </summary>
    public sealed class Box<T>(T value)
    {
        public T Value = value;
    }
}
