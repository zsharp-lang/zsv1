namespace ZSharp.VM
{
    public readonly struct Code(IEnumerable<Instruction> instructions, int stackSize)
    {
        public readonly Instruction[] Instructions { get; } = instructions.ToArray();

        public readonly int StackSize { get; } = stackSize;
    }
}
