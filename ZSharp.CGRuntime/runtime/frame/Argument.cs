namespace ZSharp.CGRuntime
{
    public sealed class Argument(CGObject @object)
    {
        public CGObject Object { get; set; } = @object;

        public string? Name { get; set; }

        public Argument(string? name, CGObject @object)
            : this(@object)
        {
            Name = name;
        }
    }
}
