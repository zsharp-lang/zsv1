namespace ZSharp.Compiler
{
    /// <summary>
    /// Whether a candidate is a better choice than another one, when an operation applies
    /// to both.
    /// </summary>
    public delegate bool IsMoreSpecific(CompilerObject candidate, CompilerObject other);

    partial class Overloading
    {
        public IsMoreSpecific IsMoreSpecific { get; set; } = Dispatcher.IsMoreSpecific;
    }
}
