namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Refers to an instance from inside code. The instance is resolved on use, so
    /// it may not exist yet when the code is built.
    /// </summary>
    public sealed class Reference(Func<Instance> reference)
        : Instance
    {
        public Instance Value => reference();
    }
}
