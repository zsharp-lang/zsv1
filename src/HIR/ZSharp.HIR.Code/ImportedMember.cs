namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A name an <see cref="Import"/> brings in. Loading it loads its import, then reads
    /// its name off the result, so it is only known once its import is loaded, not while
    /// compiling. It is a definition for the same reason <see cref="Import"/> is.
    /// </summary>
    public sealed class ImportedMember
        : Instance
        , HIR.Definition
    {
        public required Import Import { get; set; }

        public required string Name { get; set; }
    }
}
