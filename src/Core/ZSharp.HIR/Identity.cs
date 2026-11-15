namespace ZSharp.HIR
{
    /// <summary>
    /// Something that is loaded once and kept: loading it again gives the same CO,
    /// rather than building another one. A <see cref="Definition"/> is one kind, a named
    /// slot a container holds; not everything with an identity is one.
    /// </summary>
    public interface Identity
    {
    }
}
