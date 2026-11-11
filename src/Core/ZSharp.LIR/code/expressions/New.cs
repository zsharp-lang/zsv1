namespace ZSharp.LIR
{
    /// <summary>
    /// Produces a structure value from its field values, in field order.
    /// </summary>
    public sealed class New(Structure structure, params IEnumerable<Expression> fields)
        : Expression
    {
        public Structure Structure { get; set; } = structure;

        public List<Expression> Fields { get; } = [.. fields];
    }
}
