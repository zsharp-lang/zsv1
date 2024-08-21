namespace ZSharp.CGObjects
{
    public sealed class Module(string? name)
        : CGObject
    {
        public IR.Module? IR { get; set; }

        public RTFunction InitFunction { get; } = new(null);

        public string? Name { get; set; } = name;

        public CGCode Content { get; } = [];
    }
}
