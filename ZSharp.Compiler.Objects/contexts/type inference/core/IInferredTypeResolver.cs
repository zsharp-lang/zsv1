using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public interface IInferredTypeResolver
    {
        public IType Resolve(Compiler.Compiler compiler, InferredType inferredType);
    }
}
