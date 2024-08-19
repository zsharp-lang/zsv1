namespace ZSharp.CGRuntime.LLVM
{
    internal struct Instruction(OpCode opCode)
    {
        public OpCode OpCode { get; } = opCode;

        public object? Operand { get; set; }

        public Instruction(OpCode opCode, object? operand)
            : this(opCode)
        {
            Operand = operand;
        }

        public readonly T As<T>()
            => (T)Operand!;
    }
}
