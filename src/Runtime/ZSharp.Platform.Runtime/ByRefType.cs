namespace ZSharp.Platform.Runtime
{
    /// <summary>
    /// The CLR's reference to a variable of a type, as a <c>ref</c> or <c>out</c> parameter
    /// takes, of a type that may be one the runtime is still building.
    /// </summary>
    public sealed class ByRefType(LIR.Type target)
        : LIR.Type
    {
        public LIR.Type Target { get; } = target;
    }
}
