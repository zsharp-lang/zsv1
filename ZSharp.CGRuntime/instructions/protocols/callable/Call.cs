namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Call(int argumentCount) : Instruction
    {
        public int ArgumentCount { get; } = argumentCount;
    }
}
