using CommonZ.Utils;
using ZSharp.IR.VM;

namespace ZSharp.CTRuntime
{
    public sealed class CodeCombiner
    {
        public Collection<Instruction> Code { get; } = [];

        public int StackSize { get; private set; } = 0;

        public Stack<VM.ZSObject> Types { get; } = new();

        public VM.ZSObject Type => Types.Count > 0 ? Types.Peek() : VM.TypeSystem.Void;

        public void Add(Instruction instruction)
        {
            Code.Add(instruction);
        }

        public void Add(Code code)
        {
            Code.AddRange(code.Instructions);
            if (code.StackSize > StackSize)
                StackSize = code.StackSize;
            if (code.Type != VM.TypeSystem.Void)
            {
                Types.Push(code.Type);
                StackSize++;
            }
        }

        public Code Create()
        {
            return new(StackSize, Code, Type);
        }
    }
}
