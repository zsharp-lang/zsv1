namespace ZSharp.IR
{
    public sealed class ConstructorReference(Constructor constructor)
        :  ICallable
    {
        public Constructor Constructor { get; set; } = constructor;

        public required TypeReference OwningType { get; set; }

        public Signature Signature => Constructor.Method.Signature;

        public bool HasBody => Constructor.Method.HasBody;

        public ICallableBody? Body => Constructor.Method.HasBody ? Constructor.Method.Body : null;
    }
}
