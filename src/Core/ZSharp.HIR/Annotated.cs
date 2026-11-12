namespace ZSharp.HIR
{
    /// <summary>
    /// A capability of a definition: what has been said of it, as a decorator says a
    /// function is an entry point. An annotation is data only, for whatever reads the
    /// definition, as a framework that builds a module, to understand.
    /// </summary>
    public interface Annotated
    {
        public List<object> Annotations { get; }
    }
}
