namespace ZSharp.VM
{
    internal sealed class VirtualMachine(IRLoader loader, int minCallStackSize = 1024)
    {
        private readonly List<Frame> _callStack = new(minCallStackSize);
        private readonly IRLoader _loader = loader;

        public Frame CurrentFrame => _callStack[^1];

        public IRLoader IRLoader => _loader;

        public ZSObject? Evaluate(Instruction[] code, int stackSize)
        {
            return Execute(code, stackSize).PeekOrNull();
        }

        public Frame Execute(Instruction[] code, int stackSize)
        {
            Frame frame;
            _callStack.Add(frame = Frame.NoArguments(0, code, stackSize));

            Run();
            ReturnVoid();

            return frame;
        }

        public void ExecuteInstruction()
        {
            ExecuteInstruction(CurrentFrame.NextInstruction());
        }

        public void ExecuteInstruction(Instruction instruction)
        {
            switch (instruction.opCode)
            {
                case OpCode.Nop: break;
                case OpCode.Push: CurrentFrame.Push(instruction.As<ZSObject>()); break;
                case OpCode.Pop: CurrentFrame.Pop(); break;
                case OpCode.Dup: CurrentFrame.Push(CurrentFrame.Peek()); break;
                case OpCode.GetArgument: 
                    CurrentFrame.Push(CurrentFrame.Argument(instruction.As<int>())); break;
                case OpCode.SetArgument:
                    CurrentFrame.Argument(instruction.As<int>(), CurrentFrame.Pop()); break;
                case OpCode.GetLocal:
                    CurrentFrame.Push(CurrentFrame.Local(instruction.As<int>())); break;
                case OpCode.SetLocal:
                    CurrentFrame.Local(instruction.As<int>(), CurrentFrame.Pop()); break;
                case OpCode.Jump:
                    CurrentFrame.JumpBy(instruction.As<int>()); break;
                case OpCode.JumpIf:
                    throw new NotImplementedException();
                case OpCode.Call: Call(instruction); break;
                case OpCode.CallInternal: CallInternal(instruction); break;
                case OpCode.Return: ReturnValue(); break;
                case OpCode.ReturnVoid: ReturnVoid(); break;
                case OpCode.GetField: GetField(instruction.As<int>()); break;
                case OpCode.SetField: throw new NotImplementedException();
                case OpCode.LoadObjectFromMetadata: 
                    CurrentFrame.Push(IRLoader.Load(instruction.As<IR.IRObject>())); break;
                default: throw new InvalidInstructionException(instruction);
            }
        }

        public void Run()
        {
            while (!CurrentFrame.IsEndOfProgram)
                ExecuteInstruction();
        }

        private void Call(Instruction instruction)
        {
            int argc = instruction.As<int>();

            var function = CurrentFrame.Pop() as ZSFunction ?? throw new Exception();

            var args = new ZSObject[argc];
            for (var i = args.Length - 1; i >= 0; i--)
                args[i] = CurrentFrame.Pop();

            var frame = new Frame(args, function.LocalCount, function.Code, function.StackSize);

            _callStack.Add(frame);
        }

        private void CallInternal(Instruction instruction)
        {
            int argc = instruction.As<int>();

            var function = CurrentFrame.Pop() as ZSInternalFunction ?? throw new Exception();

            var args = new ZSObject[argc];
            for (var i = args.Length - 1; i >= 0; i--)
                args[i] = CurrentFrame.Pop();

            var result = function.Call(args);

            ReturnVoid();

            if (result is not null)
                CurrentFrame.Push(result);
        }

        private void GetField(int index)
        {
            if (CurrentFrame.Pop() is not ZSStruct @struct)
                throw new();

            CurrentFrame.Push(@struct.GetField(index));
        }

        private void ReturnValue()
        {
            var result = CurrentFrame.Pop();
            ReturnVoid();
            CurrentFrame.Push(result);
        }

        private void ReturnVoid()
        {
            _callStack.RemoveAt(_callStack.Count - 1);
        }
    }
}
