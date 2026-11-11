namespace ZSharp.HIR.Code
{
    /// <summary>
    /// The value of what an expression gives, as getting a value is: a definition's
    /// value, as a class's type object. What a <see cref="Get"/> gives is the instance
    /// itself.
    /// </summary>
    public sealed class GetValue
        : Expression
    {
        public required Expression Of { get; set; }
    }
}
