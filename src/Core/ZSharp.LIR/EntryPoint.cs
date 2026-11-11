namespace ZSharp.LIR
{
    /// <summary>
    /// What loads a module into the environment that imports it, as a plugin is: a
    /// function of the module, called once, with nothing, when the module is imported.
    /// It isn't what initializes the module, which runs before anything of it is used.
    /// </summary>
    public sealed class EntryPoint(Function function)
        : Metadata
    {
        public Function Function { get; } = function;
    }
}
