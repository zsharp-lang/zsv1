namespace ZSharp.Platform.Runtime.Loaders
{
    internal abstract class LoaderBase(EmitLoader loader)
    {
        public EmitLoader Loader { get; } = loader;
    }
}
