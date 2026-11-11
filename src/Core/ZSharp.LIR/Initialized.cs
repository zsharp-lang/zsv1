namespace ZSharp.LIR
{
    /// <summary>
    /// A capability of a definition: code that runs once to initialize it, before any of
    /// it is used. Whatever is lowered into it can add to that code, as a global its
    /// initial value. A target runs it as it initializes what it loads.
    /// </summary>
    public interface Initialized
    {
        public Code Initializer { get; }
    }
}
