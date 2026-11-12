namespace ZSharp.LIR
{
    /// <summary>
    /// An annotation of what a definition was lowered from, kept: a platform writes it
    /// its own way, so it is there as the code runs.
    /// </summary>
    public sealed class KeptAnnotation(object value)
        : Metadata
    {
        public object Value { get; } = value;
    }
}
