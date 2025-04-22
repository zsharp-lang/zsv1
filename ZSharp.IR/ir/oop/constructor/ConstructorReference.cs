namespace ZSharp.IR
{
    public sealed class ConstructorReference(Constructor constructor)
        :  ICallable
    {
        public Constructor Member { get; set; } = constructor;

        public required OOPTypeReference OwningType { get; set; }

        public Signature Signature => Member.Method.Signature;

        public bool HasBody => Member.Method.HasBody;

        public ICallableBody? Body => Member.Method.HasBody ? Member.Method.Body : null;
    }
}
