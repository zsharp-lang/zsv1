namespace ZSharp.IR
{
    public sealed class ValueType(string? name) : TypeDefinition
    {
        public string? Name { get; set; } = name;
    }
}
