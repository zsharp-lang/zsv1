using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public class SimpleInferenceContext(Compiler.Compiler compiler)
        : IContext
        , ITypeInferenceContext
    {
        private readonly Compiler.Compiler compiler = compiler;

        IContext? IContext.Parent { get; set; }

        InferredType ITypeInferenceContext.CreateInferredType(IInferredTypeResolver? resolver)
            => (this as ITypeInferenceContext).CreateInferredType(compiler, resolver);
    }
}
