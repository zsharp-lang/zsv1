namespace ZSharp.Compiler.ILLoader
{
    internal sealed partial class ModuleLoader(ILLoader loader)
    {
        public ILLoader Loader { get; } = loader;
    }
}
