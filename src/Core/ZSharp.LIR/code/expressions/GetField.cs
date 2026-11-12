namespace ZSharp.LIR
{
    /// <summary>
    /// Reads a field. The target is a structure value or a <see cref="Reference"/>
    /// to one.
    /// </summary>
    public sealed class GetField(Expression target, Field field)
        : Expression
    {
        public Expression Target { get; set; } = target;

        public Field Field { get; set; } = field;
    }
}
