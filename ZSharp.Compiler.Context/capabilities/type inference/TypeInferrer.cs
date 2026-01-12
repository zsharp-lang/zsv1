namespace ZSharp.Compiler
{
    public abstract class TypeInferrer
        : CompilerObject
    {
        public ITypeInferenceContext Context { get; internal set; }
    }
}
