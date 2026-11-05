namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Refers to an instance from inside code. The instance may not exist yet when
    /// the code is built, so it can be set later.
    /// </summary>
    public sealed class Reference
        : Instance
    {
        public Instance? Value { get; set; }
    }
}
