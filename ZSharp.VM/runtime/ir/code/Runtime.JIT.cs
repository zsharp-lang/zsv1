using CommonZ.Utils;
using ZSharp.IR;
using ZSharp.IR.VM;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        public Code Assemble(IEnumerable<IR.VM.Instruction> code, Function? function = null, int? stackSize = null)
        {
            List<(int, IR.VM.Instruction)> jumpTable = [];
            Dictionary<int, int> offsetMap = [];

            int maxStackSize = stackSize ?? ((function?.HasBody ?? false) ? function.Body.StackSize : 0);
            List<Instruction> result = [];

            foreach (var (index, instruction) in new Enumerate<IR.VM.Instruction>(code))
            {
                offsetMap.Add(index, result.Count);
                switch (instruction)
                {
                    case Call call:
                        result.Add(new(OpCode.Call, Get(call.Function)));
                        break;
                    case CallIndirect callIndirect:
                        result.Add(new(OpCode.Call, callIndirect.Signature.Length));
                        break;
                    case CallInternal callInternal:
                        result.Add(new(OpCode.CallInternal, GetInternalFunction(callInternal.Function)));
                        break;
                    case Jump jump:
                        result.Add(new(OpCode.Jump, 0));
                        jumpTable.Add((result.Count - 1, jump.Target));
                        break;
                    case GetArgument getArgument:
                        result.Add(new(OpCode.GetArgument, getArgument.Argument.Index));
                        break;
                    case SetArgument setArgument:
                        result.Add(new(OpCode.SetArgument, setArgument.Argument.Index));
                        break;
                    case GetGlobal getGlobal:
                        result.Add(new(OpCode.LoadObjectFromMetadata, getGlobal.Global.Module!));
                        result.Add(new(OpCode.GetField, getGlobal.Global.Index));
                        break;
                    case SetGlobal setGlobal:
                        result.Add(new(OpCode.LoadObjectFromMetadata, setGlobal.Global.Module!));
                        result.Add(new(OpCode.SetField, setGlobal.Global.Index));
                        maxStackSize++;
                        break;
                    case GetObject getObject:
                        result.Add(new(OpCode.LoadObjectFromMetadata, getObject.IR));
                        break;
                    case Pop _:
                        result.Add(new(OpCode.Pop));
                        break;
                    case PutBoolean putBoolean:
                        result.Add(new(OpCode.Push, putBoolean.Value ? True : False));
                        break;
                    case PutFloat32 putFloat32:
                        result.Add(new(OpCode.Push, new ZSFloat32(putFloat32.Value, TypeSystem.Float32)));
                        break;
                    case PutInt32 putInt32:
                        result.Add(new(OpCode.Push, new ZSInt32(putInt32.Value, TypeSystem.Int32)));
                        break;
                    case PutString putString:
                        result.Add(new(OpCode.Push, new ZSString(putString.Value, TypeSystem.String)));
                        break;
                    case Return _:
                        if (function is null) throw new InvalidOperationException();
                        result.Add(new(function.ReturnType == RuntimeModule.TypeSystem.Void ? OpCode.ReturnVoid : OpCode.Return));
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

            return new([.. result], maxStackSize);
        }
    }
}
