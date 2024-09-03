namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Get(string name) : Instruction
    {
        public string Name { get; set; } = name;

        public override string ToString()
            => $"Get(\"{Name}\")";
    }
}
