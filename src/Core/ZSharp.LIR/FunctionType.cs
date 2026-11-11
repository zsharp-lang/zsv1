namespace ZSharp.LIR
{
    /// <summary>
    /// The type of a value that refers to a function.
    /// </summary>
    public sealed class FunctionType(Signature signature)
        : Type
    {
        public Signature Signature { get; set; } = signature;
    }
}
