namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Call(int argumentCount) : Instruction
    {
        public int ArgumentCount { get; } = argumentCount;

        public override string ToString()
            => $"Call({ArgumentCount})";
    }
}
