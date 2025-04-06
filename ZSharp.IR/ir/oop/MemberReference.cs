namespace ZSharp.IR
{
    public interface MemberReference
    {
        public OOPTypeReference OwningType { get; }
    }

    public interface MemberReference<T>
        : MemberReference
        where T : IRObject
    {
        public T Member { get; }
    }
}
