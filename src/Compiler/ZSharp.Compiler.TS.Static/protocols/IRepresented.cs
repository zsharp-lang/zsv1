namespace ZSharp.Compiler
{
    /// <summary>
    /// A type the platform in use realizes as one of its own, its representation, as the
    /// CLR realizes Z#'s <c>I32</c> as its <c>Int32</c>. What realizes the platform's own
    /// types converts a value of it as it converts a value of its representation, so a
    /// value that is a CLR value type boxes where an object is expected, and isn't a
    /// reference to compare. It has none where the platform hasn't said.
    /// </summary>
    public interface IRepresented
    {
        public CompilerObject? Representation { get; }
    }
}
