using CommonZ.Utils;

namespace ZSharp.Platform.Runtime.Loaders
{
    public sealed class CodeCompiler(ICodeContext context)
    {
        public ICodeContext Context { get; } = context;

        public void CompileCode(Collection<IR.VM.Instruction> instructions)
        {
            if (Context.Is<IBranchingCodeContext>(out var branchingContext))
            {
                foreach (var instruction in instructions)
                    branchingContext.AddBranchTarget(instruction);

                foreach (var instruction in instructions)
                {
                    Context.IL.MarkLabel(branchingContext.GetBranchTarget(instruction));

                    Compile(instruction);
                }
            } else
                foreach (var instruction in instructions)
                    Compile(instruction);
        }

        private void Compile(IR.VM.Instruction instruction)
        {
            if (Context.Is<IDebuggableContext>(out var debuggable))
            {
                if (debuggable.TryGetSequencePoint(instruction, out var location))
                    Context.IL.MarkSequencePoint(
                        debuggable.Document,
                        location.StartLine,
                        location.StartColumn,
                        location.EndLine,
                        location.EndColumn
                    );
            }

            switch (instruction)
            {
                case IR.VM.Call call: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), call); break;
                case IR.VM.CallIndirect callIndirect: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), callIndirect); break;
                case IR.VM.CallVirtual callVirtual: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), callVirtual); break;
                case IR.VM.CastReference castReference: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), castReference); break;
                case IR.VM.CreateInstance createInstance: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), createInstance); break;
                case IR.VM.Dup dup: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), dup); break;
                case IR.VM.GetArgument getArgument: CodeCompiler_Impl.Compile(RequireContext<IFrameCodeContext>(), getArgument); break;
                //case IR.VM.GetClass getClass: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), getClass); break;
                case IR.VM.GetField getField: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), getField); break;
                case IR.VM.GetGlobal getGlobal: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), getGlobal); break;
                case IR.VM.GetLocal getLocal: CodeCompiler_Impl.Compile(RequireContext<IFrameCodeContext>(), getLocal); break;
                //case IR.VM.GetObject getObject: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), getObject); break;
                case IR.VM.IsNotNull isNotNull: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), isNotNull); break;
                case IR.VM.IsNull isNull: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), isNull); break;
                case IR.VM.Jump jump: CodeCompiler_Impl.Compile(RequireContext<IBranchingCodeContext>(), jump); break;
                case IR.VM.JumpIfTrue jumpIfTrue: CodeCompiler_Impl.Compile(RequireContext<IBranchingCodeContext>(), jumpIfTrue); break;
                case IR.VM.JumpIfFalse jumpIfFalse: CodeCompiler_Impl.Compile(RequireContext<IBranchingCodeContext>(), jumpIfFalse); break;
                case IR.VM.Nop nop: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), nop); break;
                case IR.VM.Pop pop: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), pop); break;
                case IR.VM.PutBoolean putBoolean: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), putBoolean); break;
                case IR.VM.PutFloat32 putFloat32: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), putFloat32); break;
                case IR.VM.PutInt32 putInt32: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), putInt32); break;
                case IR.VM.PutNull putNull: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), putNull); break;
                case IR.VM.PutString putString: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), putString); break;
                case IR.VM.Return @return: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), @return); break;
                case IR.VM.SetArgument setArgument: CodeCompiler_Impl.Compile(RequireContext<IFrameCodeContext>(), setArgument); break;
                case IR.VM.SetField setField: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), setField); break;
                case IR.VM.SetGlobal setGlobal: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), setGlobal); break;
                case IR.VM.SetLocal setLocal: CodeCompiler_Impl.Compile(RequireContext<IFrameCodeContext>(), setLocal); break;
                case IR.VM.Swap swap: CodeCompiler_Impl.Compile(RequireContext<ICodeContext>(), swap); break;

                default: throw new NotImplementedException();
            }
        }

        private T RequireContext<T>()
            where T : class, ICodeContext
            => Context.Is<T>(out var required) ? required : throw new InvalidOperationException();
    }
}
