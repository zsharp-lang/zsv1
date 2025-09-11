namespace ZSharp.Compiler.ILLoader
{
    internal sealed partial class ModuleBodyLoader(IAddMember container, ILLoader loader)
    {
        public ILLoader Loader { get; } = loader;

        public IAddMember Container { get; } = container;
    }
}
