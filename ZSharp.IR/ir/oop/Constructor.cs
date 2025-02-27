namespace ZSharp.IR
{
    public sealed class Constructor(string? name)
        : IRObject
    {
        public override Module? Module => Method.Module;

        public string? Name { get; set; } = name;

        public Method Method { get; set; }
    }
}
