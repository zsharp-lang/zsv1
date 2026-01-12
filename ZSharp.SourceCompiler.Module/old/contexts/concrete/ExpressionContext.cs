using ZSharp.Compiler;
using ZSharp.Objects;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class ExpressionContext(Compiler.Compiler compiler)
        : IContext
        , ITypeInferenceContext
    {
        private readonly Compiler.Compiler compiler = compiler;

        IContext? IContext.Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        InferredType ITypeInferenceContext.CreateInferredType(IInferredTypeResolver? resolver)
            => (this as ITypeInferenceContext).CreateInferredType(compiler, resolver);
    }
}
