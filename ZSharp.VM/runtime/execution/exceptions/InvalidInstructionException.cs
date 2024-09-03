namespace ZSharp.VM
{
    public sealed class InvalidInstructionException(Instruction instruction) : ZSRuntimeException
    {
        public Instruction Instruction { get; } = instruction;
    }
}
