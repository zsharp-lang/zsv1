namespace Package.DotNet
{
    /// <summary>
    /// A reference to a variable of a type, <c>Ref[T]</c>, which the CLR has as <c>T&amp;</c>.
    /// <c>Out[T]</c> is the same type, for a parameter the callee only assigns.
    /// </summary>
    public sealed class RefType(ZSharp.HIR.Type target)
        : TypeReference
    {
        public ZSharp.HIR.Type Target { get; set; } = target;

        public bool IsOut { get; set; }
    }
}
