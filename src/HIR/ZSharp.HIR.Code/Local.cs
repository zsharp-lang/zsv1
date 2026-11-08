namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A variable. It starts where a <see cref="Declare"/> of it is, with its initial
    /// value if it has one.
    /// </summary>
    public sealed class Local(string name, Type type)
        : Definition
        , Instance
    {
        public string Name { get; set; } = name;

        public Type Type { get; set; } = type;

        public Expression? Init { get; set; }
    }
}
