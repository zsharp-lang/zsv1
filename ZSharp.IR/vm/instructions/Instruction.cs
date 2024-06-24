namespace ZSharp.IR.VM
{
    public abstract class Instruction
    {
        public Instruction? Previous { get; internal set; }

        public Instruction? Next { get; internal set; }

        public int Index { get; internal set; }
    }
}
