namespace ZSharp.IR
{
    public sealed class ValueType(string? name) : OOPType
    {
        public string? Name { get; set; } = name;
    }
}
