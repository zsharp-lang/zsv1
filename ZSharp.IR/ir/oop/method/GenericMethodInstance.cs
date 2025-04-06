using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class GenericMethodInstance(MethodReference method)
        : MemberReference<Method>
        , ICallable
    {
        public MethodReference Method { get; set; } = method;

        public Collection<IType> Arguments { get; } = /*method.Member.HasGenericParameters*/false ? [] : Collection<IType>.Empty;

        public Method Member => Method.Member;

        public OOPTypeReference OwningType => Method.OwningType;

        public Signature Signature => Method.Signature;

        public bool HasBody => Method.HasBody;

        public ICallableBody? Body => Method.HasBody ? Method.Body : null;
    }
}
