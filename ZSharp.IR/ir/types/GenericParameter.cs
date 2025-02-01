namespace ZSharp.IR
{
    public sealed class GenericParameter(string name)
        : IType
    {
        public string Name { get; set; } = name;
    }
}
