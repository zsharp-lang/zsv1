namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Argument(string? name = null) : Instruction
    {
        public string? Name { get; set; } = name;
    }
}
