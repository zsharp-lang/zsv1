﻿using CommonZ.Utils;

namespace ZSharp.IR.VM
{
    public sealed class FunctionBody : ICallableBody
    {
        private InstructionCollection? _instructions;
        private LocalCollection? _locals;

        public Function Function { get; }

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

                Interlocked.CompareExchange(ref _locals, new(Function), null);
                return _locals;
            }
        }

        public bool HasLocals => !_locals.IsNullOrEmpty();

        public int StackSize { get; set; }

        public FunctionBody(Function function, IEnumerable<Instruction> code)
        {
            Function = function;
            _instructions = new(code);
        }

        public FunctionBody(Function function)
        {
            Function = function;
        }
    }
}
