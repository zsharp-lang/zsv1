namespace ZSharp.IR
{
    public interface OOPTypeReference : IType
    {
        public OOPTypeReference? OwningType { get; set; }

        public OOPType Definition { get; }
    }

    public interface OOPTypeReference<T> : OOPTypeReference
        where T : OOPType
    {
        public new T Definition { get; }

        OOPType OOPTypeReference.Definition {
            get => Definition;
        }
    }
}
