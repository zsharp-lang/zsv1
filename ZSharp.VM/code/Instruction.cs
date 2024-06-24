namespace ZSharp.VM
{
    public struct Instruction(OpCode opCode)
    {
        public readonly OpCode opCode = opCode;

        public object? operand;

        public Instruction(OpCode opCode, object operand)
            : this(opCode)
        {
            this.operand = operand;
        }

        public readonly T As<T>()
        {
            return (T)operand!;
        }
    }
}
