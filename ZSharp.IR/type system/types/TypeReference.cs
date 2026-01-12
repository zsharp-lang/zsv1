namespace ZSharp.IR
{
    public interface TypeReference : IType
    {
        public TypeReference? OwningType { get; set; }

        public TypeDefinition Definition { get; }
    }

    public interface TypeReference<T> : TypeReference
        where T : TypeDefinition
    {
        public new T Definition { get; }

        TypeDefinition TypeReference.Definition {
            get => Definition;
        }
    }
}
