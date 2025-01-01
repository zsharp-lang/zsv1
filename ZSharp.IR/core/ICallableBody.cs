using CommonZ.Utils;

namespace ZSharp.IR
{
    public interface ICallableBody
    {
        public Collection<VM.Instruction> Instructions { get; }

        public bool HasInstructions { get; }

        public Collection<VM.Local> Locals { get; }

        public bool HasLocals { get; }
    }
}
