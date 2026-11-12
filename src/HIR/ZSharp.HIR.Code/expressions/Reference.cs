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

        /// <summary>
        /// The kind of definition it will refer to, as what describes its node's type, if
        /// that is known before it does, as a definition being built knows.
        /// </summary>
        public Instance? Kind { get; set; }
    }
}
