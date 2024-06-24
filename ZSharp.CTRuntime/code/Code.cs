using CommonZ.Utils;
using ZSharp.IR.VM;

namespace ZSharp.CTRuntime
{
    public class Code(int stackSize, Collection<Instruction> instructions, VM.ZSObject type)
    {
        public static readonly Code Empty = new(0, [], VM.TypeSystem.Void);

        public int StackSize = stackSize;

        public Collection<Instruction> Instructions = instructions;

        public VM.ZSObject Type = type;
    }
}
