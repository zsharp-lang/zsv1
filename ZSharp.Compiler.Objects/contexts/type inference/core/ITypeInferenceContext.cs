using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public interface ITypeInferenceContext
        : IContext
    {
        public InferredType CreateInferredType(IInferredTypeResolver? resolver = null);

        public InferredType CreateInferredType(
            Compiler.Compiler compiler,
            IInferredTypeResolver? resolver = null
        )
            => new(compiler, this)
            {
                Resolver = resolver ?? new CommonBaseType()
            };
    }
}
