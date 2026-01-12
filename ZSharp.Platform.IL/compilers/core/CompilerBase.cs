namespace ZSharp.Platform.IL
{
    internal abstract class CompilerBase(Context context)
    {
        public Context Context { get; } = context;
    }
}
