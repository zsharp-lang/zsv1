namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Executes a single instruction in the context of the current frame.
        /// </summary>
        /// <param name="instruction">The instruction to execute.</param>
        private void Execute(Instruction instruction)
        {
            switch (instruction.opCode)
            {
                case OpCode.Call: ExecuteCall(instruction); break;
                case OpCode.CallInternal: ExecuteCallInternal(instruction); break;
                case OpCode.CallVirtual: ExecuteCallVirtual(instruction); break;
                case OpCode.Dup: ExecuteDup(instruction); break;
                case OpCode.GetArgument: ExecuteGetArgument(instruction); break;
                case OpCode.GetField: ExecuteGetField(instruction); break;
                case OpCode.GetLocal: ExecuteGetLocal(instruction); break;
                case OpCode.GetMember: ExecuteGetMember(instruction); break;
                case OpCode.Jump: ExecuteJump(instruction); break;
                case OpCode.JumpIf: ExecuteJumpIf(instruction); break;
                case OpCode.LoadObjectFromMetadata: ExecuteLoadObjectFromMetadata(instruction); break;
                case OpCode.NewArray: ExecuteNewArray(instruction); break;
                case OpCode.Nop: ExecuteNop(); break;
                case OpCode.Pop: ExecutePop(instruction); break;
                case OpCode.Push: ExecutePush(instruction); break;
                case OpCode.Return: ExecuteReturn(); break;
                case OpCode.ReturnVoid: ExecuteReturnVoid(); break;
                case OpCode.SetArgument: ExecuteSetArgument(instruction); break;
                case OpCode.SetField: ExecuteSetField(instruction); break;
                case OpCode.SetLocal: ExecuteSetLocal(instruction); break;
                case OpCode.SetMember: ExecuteSetMember(instruction); break;
                default: throw new InvalidInstructionException(instruction);
            }
        }
    }
}
