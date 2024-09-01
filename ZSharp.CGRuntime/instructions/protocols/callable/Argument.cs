namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Argument(string? name = null) : Instruction
    {
        public string? Name { get; set; } = name;

        public override string ToString()
            => Name is null
            ? base.ToString()
            : $"Argument(\"{Name}\")"
            ;
    }
}
