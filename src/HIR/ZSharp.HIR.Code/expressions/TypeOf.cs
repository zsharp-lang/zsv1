namespace ZSharp.HIR.Code
{
    /// <summary>
    /// The type of a value, as the compiler finds it once the code is loaded: a type that
    /// is described by the code that gives the value.
    /// </summary>
    public sealed class TypeOf
        : Expression
        , Type
    {
        public required Expression Value { get; set; }
    }
}
