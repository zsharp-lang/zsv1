namespace ZSharp.IR
{
    public sealed class Constructor(string? name)
    {
        public string? Name { get; set; } = name;

        public Method Method { get; set; }
    }
}
