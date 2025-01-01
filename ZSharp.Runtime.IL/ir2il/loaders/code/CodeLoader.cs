namespace ZSharp.Runtime.NET.IR2IL
{
    internal sealed partial class CodeLoader(IRLoader loader, IR.ICallable code, IL.Emit.ILGenerator method)
        : BaseIRLoader<IR.ICallable, IL.Emit.ILGenerator>(loader, code, method)
        , ICodeLoader
    {
        private readonly Dictionary<IR.VM.Instruction, IL.Emit.Label> labels = [];

        public Stack<Type> Stack => _stack;

        public Dictionary<IR.Parameter, Parameter> Args { get; } = [];

        public Dictionary<IR.VM.Local, IL.Emit.LocalBuilder> Locals { get; } = [];

        public void Load()
        {
            CompileCode();
        }

        private void CompileCode()
        {
            if (!Input.HasBody || !Input.Body.HasInstructions) return;

            foreach (var instruction in Input.Body.Instructions)
                labels[instruction] = Output.DefineLabel();


            foreach (var instruction in Input.Body.Instructions)
                Compile(instruction);
        }

        public void Push(Type type)
            => _stack.Push(type);

        public void Pop()
            => _stack.Pop();

        public void Pop(Type type)
        {
            if (_stack.Pop() != type)
                throw new($"Expected type {type} on the stack!");
        }

        public void Pop(params Type[] types)
        {
            foreach (var type in types.Reverse())
                Pop(type);
        }

        private void Compile(IR.VM.Instruction instruction)
        {
            Output.MarkLabel(labels[instruction]);

            switch (instruction)
            {
                case IR.VM.Call call: Compile(call); break;
                case IR.VM.CallIndirect callIndirect: Compile(callIndirect); break;
                case IR.VM.CallInternal callInternal: Compile(callInternal); break;
                case IR.VM.CallVirtual callVirtual: Compile(callVirtual); break;
                case IR.VM.CreateInstance createInstance: Compile(createInstance); break;
                case IR.VM.Dup dup: Compile(dup); break;
                case IR.VM.GetArgument getArgument: Compile(getArgument); break;
                case IR.VM.GetField getField: Compile(getField); break;
                case IR.VM.GetGlobal getGlobal: Compile(getGlobal); break;
                case IR.VM.GetLocal getLocal: Compile(getLocal); break;
                case IR.VM.GetObject getObject: Compile(getObject); break;
                case IR.VM.Jump jump: Compile(jump); break;
                case IR.VM.JumpIfTrue jumpIfTrue: Compile(jumpIfTrue); break;
                case IR.VM.JumpIfFalse jumpIfFalse: Compile(jumpIfFalse); break;
                case IR.VM.Nop nop: Compile(nop); break;
                case IR.VM.Pop pop: Compile(pop); break;
                case IR.VM.PutBoolean putBoolean: Compile(putBoolean); break;
                case IR.VM.PutFloat32 putFloat32: Compile(putFloat32); break;
                case IR.VM.PutInt32 putInt32: Compile(putInt32); break;
                case IR.VM.PutNull putNull: Compile(putNull); break;
                case IR.VM.PutString putString: Compile(putString); break;
                case IR.VM.Return @return: Compile(@return); break;
                case IR.VM.SetArgument setArgument: Compile(setArgument); break;
                case IR.VM.SetField setField: Compile(setField); break;
                case IR.VM.SetGlobal setGlobal: Compile(setGlobal); break;
                case IR.VM.SetLocal setLocal: Compile(setLocal); break;
                case IR.VM.Swap swap: Compile(swap); break;

                default: throw new NotImplementedException();
            }
        }
    }
}
