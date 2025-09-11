namespace ZSharp.IR
{
    public sealed class Constructor(string? name)
        : IRDefinition
        , IModuleMember
    {
        public Module? Module => Method.Module;

        public string? Name { get; set; } = name;

        public required Method Method { get; set; }
    }
}
