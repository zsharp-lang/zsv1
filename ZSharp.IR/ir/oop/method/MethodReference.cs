namespace ZSharp.IR
{
    public class MethodReference(Method method)
        : ICallable
    {
        public Method Method { get; set; } = method;

        public required OOPTypeReference OwningType { get; set; }

        public Signature Signature { get; init; } = method.Signature;

        public bool HasBody => Method.HasBody;

        public ICallableBody? Body => Method.HasBody ? Method.Body : null;
    }
}
