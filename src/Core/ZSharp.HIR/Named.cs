namespace ZSharp.HIR
{
    /// <summary>
    /// A capability of a definition: it has a name of its own, by which what holds it,
    /// as a module, can be found to have it. A definition need not have one.
    /// </summary>
    public interface Named
    {
        public string Name { get; }
    }
}
