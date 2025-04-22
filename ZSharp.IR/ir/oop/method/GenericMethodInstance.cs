using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class GenericMethodInstance(Method method)
        : MethodReference(method)
        , ICallable
    {
        public Collection<IType> Arguments { get; } = /*method.Member.HasGenericParameters*/false ? [] : Collection<IType>.Empty;
    }
}
