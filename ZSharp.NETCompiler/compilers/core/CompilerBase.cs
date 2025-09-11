namespace ZSharp.NETCompiler
{
    internal abstract class CompilerBase(Context context)
    {
        public Context Context { get; } = context;
    }
}
