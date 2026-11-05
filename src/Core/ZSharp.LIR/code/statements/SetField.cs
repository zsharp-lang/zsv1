namespace ZSharp.LIR
{
    /// <summary>
    /// Writes a field. The target is a <see cref="Reference"/> to a structure, or
    /// a variable, global or field that holds one inline.
    /// </summary>
    public sealed class SetField(Expression target, Field field, Expression value)
        : Statement
    {
        public Expression Target { get; set; } = target;

        public Field Field { get; set; } = field;

        public Expression Value { get; set; } = value;
    }
}
