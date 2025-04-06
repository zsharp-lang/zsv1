namespace ZSharp.IR
{
    public sealed class MethodReference(Method method)
        : MemberReference<Method>
        , ICallable
    {
        public Method Member { get; set; } = method;

        public required OOPTypeReference OwningType { get; set; }

        public Signature Signature => Member.Signature;

        public bool HasBody => Member.HasBody;

        public ICallableBody? Body => Member.HasBody ? Member.Body : null;
    }
}
