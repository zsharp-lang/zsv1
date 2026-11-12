namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A variable of code around a function, as the function uses it. The code around
    /// and the function both refer to the capture, and each loads it in its own way. It
    /// holds what it captures, so it can be turned back into it.
    /// </summary>
    public sealed class Capture(Variable captured)
        : Definition
        , Variable
        , Instance
    {
        public Variable Captured { get; } = captured;
    }
}
