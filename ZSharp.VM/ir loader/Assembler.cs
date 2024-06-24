using CommonZ.Utils;
using ZSharp.IR;
using ZSharp.IR.VM;

namespace ZSharp.VM
{
    internal sealed class Assembler(RuntimeModule runtimeModule)//(IRLoader loader)
    {
        private readonly RuntimeModule runtime = runtimeModule;
        //public IRLoader IRLoader { get; } = loader;

        public Instruction[] Assemble(IEnumerable<IR.VM.Instruction> code, Function? function = null)
        {
            List<(int, IR.VM.Instruction)> jumpTable = [];
            Dictionary<int, int> offsetMap = [];

            List<Instruction> result = [];

            foreach (var (index, instruction) in new Enumerate<IR.VM.Instruction>(code))
            {
                offsetMap.Add(index, result.Count);
                switch (instruction)
                {
                    case Call call:
                        result.Add(new(OpCode.Call, call.Function.Signature.Length));
                        break;
                    case CallIndirect callIndirect:
                        result.Add(new(OpCode.Call, callIndirect.Signature.Length));
                        break;
                    case CallInternal callInternal:
                        result.Add(new(OpCode.LoadObjectFromMetadata, callInternal.Function));
                        result.Add(new(OpCode.CallInternal, callInternal.Function.Signature.Length));
                        break;
                    case Jump jump:
                        result.Add(new(OpCode.Jump, 0));
                        jumpTable.Add((result.Count - 1, jump.Target));
                        break;
                    case GetArgument getArgument:
                        result.Add(new(OpCode.GetArgument, getArgument.Argument.Index));
                        break;
                    case GetGlobal getGlobal:
                        result.Add(new(OpCode.LoadObjectFromMetadata, getGlobal.Global.Module));
                        result.Add(new(OpCode.GetField, getGlobal.Global.Index));
                        break;
                    case GetObject getObject:
                        result.Add(new(OpCode.LoadObjectFromMetadata, getObject.IR));
                        break;
                    case Pop _:
                        result.Add(new(OpCode.Pop));
                        break;
                    case PutString putString:
                        result.Add(new(OpCode.Push, new ZSString(putString.Value)));
                        break;
                    case Return _:
                        if (function is null) throw new InvalidOperationException();
                        result.Add(new(function.ReturnType == runtime.TypeSystem.Void ? OpCode.ReturnVoid : OpCode.Return));
                        break;
                    default:
                        break;
                }
            }

            foreach (var (instructionIndex, targetInstruction) in jumpTable)
            {
                var instruction = result[instructionIndex];
                var offsetFromStart = offsetMap[targetInstruction.Index];
                var offsetFromCurrent = offsetFromStart - instructionIndex;
                instruction.operand = offsetFromCurrent;
                result[instructionIndex] = instruction;
            }

            return [.. result];
        }
    }
}
