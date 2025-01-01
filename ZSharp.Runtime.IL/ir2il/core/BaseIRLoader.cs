namespace ZSharp.Runtime.NET.IR2IL
{
    internal abstract class BaseIRLoader(IRLoader loader)
    {
        public IRLoader Loader { get; } = loader;

        public Context Context => Loader.Context;
    }

    internal abstract class BaseIRLoader<In, Out>(IRLoader loader, In @in, Out @out)
        : BaseIRLoader(loader)
    {
        public In Input { get; } = @in;

        public Out Output { get; } = @out;
    }
}
