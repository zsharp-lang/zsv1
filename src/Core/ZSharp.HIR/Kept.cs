namespace ZSharp.HIR
{
    /// <summary>
    /// A capability of an annotation's type: an annotation of it is kept as a definition
    /// lowers, onto what it lowers to, and a platform writes it its own way, so it is
    /// there as the code runs. Its state is constants a platform can write.
    /// </summary>
    public interface Kept
    {
    }
}
