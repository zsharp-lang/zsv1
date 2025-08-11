namespace ZSharp.IR
{
    public sealed class Constructor(string? name)
        : IRDefinition
    {
        public override Module? Module => Method.Module;

        public string? Name { get; set; } = name;

        public Method Method { get; set; }
    }
}
