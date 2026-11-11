namespace ZSharp.HIR
{
    /// <summary>
    /// A capability of a definition: it is an operator, the one it says. What that is, the
    /// functions a piece of syntax stands for or an operation of the language, is for what
    /// holds the definition to understand.
    /// </summary>
    public interface OperatorDefinition
    {
        public object? Operator { get; set; }
    }
}
