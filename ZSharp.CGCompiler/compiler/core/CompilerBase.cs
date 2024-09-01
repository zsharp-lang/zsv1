namespace ZSharp.CGCompiler
{
    internal abstract class CompilerBase(Context context)
    {
        public Context Context { get; } = context;

        protected void Emit(CGCode code)
            => Context.Emit(code);
    }
}
