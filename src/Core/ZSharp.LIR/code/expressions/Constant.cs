namespace ZSharp.LIR
{
    /// <summary>
    /// A value known at compile time. The platform interprets it according to
    /// <see cref="Type"/>, so LIR never looks inside it.
    /// </summary>
    public sealed class Constant(Type type, object? value)
        : Expression
    {
        public Type Type { get; set; } = type;

        public object? Value { get; set; } = value;
    }
}
