namespace ZSharp.LIR
{
    public abstract class Definition
    {
        public string? Name { get; set; }

        public List<Metadata> Metadata { get; } = [];
    }
}
