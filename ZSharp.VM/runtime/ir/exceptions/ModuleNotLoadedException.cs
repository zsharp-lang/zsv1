namespace ZSharp.VM
{
    /// <summary>
    /// Thrown when an object belonging to a module is attempted to be loaded into the runtime
    /// while the module itself has not been loaded.
    /// </summary>
    /// <param name="origin">The object that was attempted to be loaded into the runtime.</param>
    public class ModuleNotLoadedException(IR.IRObject origin) 
        : IRObjectNotLoadedException(origin.Module!)
    {
        public IR.IRObject Origin => origin;
    }

    /// <summary>
    /// Thrown when an object belonging to a module is attempted to be loaded into the runtime
    /// while the module itself has not been loaded.
    /// </summary>
    /// <param name="origin">The object that was attempted to be loaded into the runtime.</param>
    public sealed class ModuleNotLoadedException<T>(T origin)
        : ModuleNotLoadedException(origin)
        where T : IR.IRObject
    {
        public new T Origin => origin;
    }
}
