namespace ZSharp.Platform.Runtime
{
    /// <summary>
    /// The address of a variable: a local, a parameter, a global, a field or what a
    /// reference holds, or of the variable statements end in, once they have run. It is how the CLR passes a variable by reference, so it is a node
    /// of this platform, not of LIR.
    /// </summary>
    public sealed class AddressOf(LIR.Expression variable, LIR.Type type)
        : LIR.Expression
    {
        public LIR.Expression Variable { get; set; } = variable;

        /// <summary>
        /// The type of the address, a reference to the variable's type.
        /// </summary>
        public LIR.Type Type { get; set; } = type;

        /// <summary>
        /// Whether an expression reads a variable that has an address.
        /// </summary>
        public static bool Has(LIR.Expression expression)
            => expression is LIR.GetLocal or LIR.GetGlobal or LIR.GetField or LIR.Load
            || expression is LIR.Sequence sequence && Has(sequence.Result);
    }
}
