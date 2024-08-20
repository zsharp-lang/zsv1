using System.Diagnostics.CodeAnalysis;

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

        public readonly bool Is<T>([NotNullWhen(true)] out T? value)
            where T : class
            => (value = Operand as T) is not null;
    }
}
