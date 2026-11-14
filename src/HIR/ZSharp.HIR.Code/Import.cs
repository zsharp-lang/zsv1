namespace ZSharp.HIR.Code
{
    /// <summary>
    /// An import: an expression that, when loaded, gives what was imported. Loading it
    /// evaluates <see cref="From"/> and expects an <see cref="HIR.Instance"/> back, which
    /// is then loaded to the CO its <see cref="ImportedMember"/>s read from. Nothing of
    /// it is known while compiling; it is a definition only so loading it once is kept,
    /// as any other is.
    /// </summary>
    public sealed class Import
        : Instance
        , HIR.Definition
    {
        public required Expression From { get; set; }
    }
}
