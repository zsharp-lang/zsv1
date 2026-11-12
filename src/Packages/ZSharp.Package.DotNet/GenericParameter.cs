namespace Package.DotNet
{
    /// <summary>
    /// A type parameter of a generic definition, as in the <c>T</c> of <c>Box[T]</c>. Inside
    /// the definition it is a type like any other.
    /// </summary>
    public sealed class GenericParameter(string name)
        : TypeReference
    {
        public string Name { get; set; } = name;
    }
}
