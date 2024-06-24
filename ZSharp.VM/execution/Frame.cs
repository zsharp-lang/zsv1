namespace ZSharp.VM
{
    internal sealed class Frame(ZSObject[] arguments, int numLocals, Instruction[] code, int stackSize)
    {
        private readonly ZSObject[] _arguments = arguments;
        private readonly ZSObject[] _locals = numLocals == 0 ? [] : new ZSObject[numLocals];

        private readonly ZSObject[] _stack = new ZSObject[stackSize];
        private int _sp = -1;

        private readonly Instruction[] _code = code;
        private int _ip = -1;

        public bool IsStackEmpty => _sp == -1;
        public bool IsStackFull => _sp == _stack.Length;

        public bool IsEndOfProgram => _ip == _code.Length - 1;

        public static Frame NoArguments(int numLocals, Instruction[] code, int stackSize)
        {
            return new([], numLocals, code, stackSize);
        }

        public ZSObject Argument(int index)
            => _arguments[index];

        public void Argument(int index, ZSObject value)
            => _arguments[index] = value;

        public ZSObject Local(int index)
            => _locals[index];

        public void Local(int index, ZSObject value)
            => _locals[index] = value;

        public void JumpBy(int offset)
            => _ip += offset;

        public void JumpTo(int ip)
            => _ip = ip;

        public Instruction NextInstruction()
            => IsEndOfProgram ? throw new EndOfProgramException() : _code[++_ip];

        public ZSObject Peek()
            => IsStackEmpty ? throw new StackIsEmptyException() : _stack[_sp];

        public ZSObject? PeekOrNull()
            => IsStackEmpty ? null : _stack[_sp];

        public ZSObject Pop()
            => _stack[_sp--];

        public void Push(ZSObject value)
            => _stack[++_sp] = IsStackFull ? throw new StackIsFullException() : value;
    }
}
