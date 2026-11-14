namespace ZSharp.HIR.Code
{
    /// <summary>
    /// An import: an expression that, when loaded, gives what was imported. Loading it
    /// evaluates <see cref="From"/> and expects an <see cref="HIR.Instance"/> back, which
    /// is then loaded to the CO its <see cref="ImportedMember"/>s read from. Nothing of
    /// it is known while compiling; it is an <see cref="HIR.Identity"/>, not a
    /// <see cref="HIR.Definition"/> — it is not a named slot a container holds — so
    /// loading it once is kept, the same as a definition's is.
    /// </summary>
    public sealed class Import
        : Instance
        , HIR.Identity
    {
        public required Expression From { get; set; }
    }
}
