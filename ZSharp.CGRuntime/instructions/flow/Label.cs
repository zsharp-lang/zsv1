namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Label : Instruction
    {
        public IR.VM.Nop Target { get; } = new();
    }
}
