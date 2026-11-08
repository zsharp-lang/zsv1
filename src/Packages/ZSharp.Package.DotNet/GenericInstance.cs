namespace Package.DotNet
{
    /// <summary>
    /// A generic type given its arguments, as <c>List[Int32]</c>.
    /// </summary>
    public sealed class GenericInstance(TypeReference definition)
        : ClassReference
    {
        public TypeReference Definition { get; set; } = definition;

        public List<TypeReference> Arguments { get; } = [];
    }
}
