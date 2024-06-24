namespace ZSharp.VM
{
    public sealed class InvalidInstructionException(Instruction instruction) : Exception
    {
        public Instruction Instruction { get; } = instruction;
    }
}
