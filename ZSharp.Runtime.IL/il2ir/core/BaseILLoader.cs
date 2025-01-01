namespace ZSharp.Runtime.NET.IL2IR
{
    internal abstract class BaseILLoader(ILLoader loader)
    {
        public ILLoader Loader { get; } = loader;

        public Context Context => Loader.Context;
    }

    internal abstract class BaseILLoader<In, Out>(ILLoader loader, In @in, Out @out)
        : BaseILLoader(loader)
    {
        public In Input { get; } = @in;

        public Out Output { get; } = @out;

        public abstract Out Load();
    }
}
