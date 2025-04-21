namespace ZSharp.Runtime.NET.IR2IL
{
    internal abstract class ModuleContentLoader<In, Out>(RootModuleLoader loader, In @in, Out @out)
        : BaseIRLoader<In, Out>(loader.Loader, @in, @out)
    {
        public RootModuleLoader ModuleLoader { get; } = loader;

        public Out Load()
        {
            DoLoad();

            return Output;
        }

        protected abstract void DoLoad();
    }
}
