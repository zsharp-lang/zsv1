using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class ConstructedFunction(Function function)
        : ICallable
    {
        public Function Function { get; set; } = function;

        public Collection<IType> Arguments { get; set; } = function.HasGenericParameters ? [] : Collection<IType>.Empty;

        public Signature Signature { get; init; } = function.Signature;

        public bool HasBody => Function.HasBody;

        public ICallableBody? Body => Function.HasBody ? Function.Body : null;
    }
}
