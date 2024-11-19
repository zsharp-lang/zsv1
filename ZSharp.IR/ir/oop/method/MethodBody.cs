using CommonZ.Utils;

namespace ZSharp.IR.VM
{
    public sealed class MethodBody
    {
        private InstructionCollection? _instructions;
        private LocalCollection? _locals;

        public Method Method { get; }

        public Collection<Instruction> Instructions
        {
            get
            {
                if (_instructions is not null)
                    return _instructions;

                Interlocked.CompareExchange(ref _instructions, [], null);
                return _instructions;
            }
        }

        public bool HasInstructions => !_instructions.IsNullOrEmpty();

        public Collection<Local> Locals
        {
            get
            {
                if (_locals is not null)
                    return _locals;

                Interlocked.CompareExchange(ref _locals, new(Method), null);
                return _locals;
            }
        }

        public bool HasLocals => !_locals.IsNullOrEmpty();

        public int StackSize { get; set; }

        public MethodBody(Method method, IEnumerable<Instruction> code)
        {
            Method = method;
            _instructions = new(code);
        }

        public MethodBody(Method method)
        {
            Method = method;
        }
    }
}
