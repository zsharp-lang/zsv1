namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A variable. It starts where a <see cref="Declare"/> of it is, with its initial
    /// value if it has one.
    /// </summary>
    public sealed class Local(string name, Type type)
        : Variable
        , Instance
    {
        public string Name { get; set; } = name;

        public Type Type { get; set; } = type;

        public Expression? Init { get; set; }

        /// <summary>
        /// Whether a function inside the code captures this local. The code it belongs to
        /// then stores it so that it can be shared.
        /// </summary>
        public bool IsCaptured { get; set; }
    }
}
