namespace ZSharp.CGRuntime
{
    internal sealed class Frame(IEnumerable<LLVM.Instruction> instructions)
    {
        private readonly Stack<CGObject> stack = [];

        private readonly Stack<Argument> args = [];

        private readonly LLVM.Instruction[] code = instructions.ToArray();

        private int pc = -1;

        public CGObject Pop()
            => stack.Pop();

        public CGObject Top()
            => stack.Peek();

        public void Put(CGObject obj)
            => stack.Push(obj);

        public void Arg(string? name)
            => args.Push(new(name, Pop()));

        public Argument Arg()
            => args.Pop();

        public LLVM.Instruction Instruction()
            => ++pc == code.Length ? new(LLVM.OpCode.End) : code[pc];

        public void Jump(int index)
            => pc = index;

        public CGObject[] ToArray()
            => [.. stack.Reverse()];
    }
}
