namespace ZSharp.CGRuntime
{
    public sealed class Runtime(ICodeGenerator codeGenerator, ICodeInjector codeInjector)
    {
        private readonly Stack<Frame> frames = new();
        private readonly Context context = new();

        public ICodeGenerator CodeGenerator { get; init; } = codeGenerator;

        public ICodeInjector Injector { get; init; } = codeInjector;

        public IErrorHandler ErrorHandler { get; set; } = new DefaultErrorHandler();

        private Frame Frame => frames.Peek();

        public CGObject[] Run(IEnumerable<HLVM.Instruction> code)
        {
            return Run(JIT.Compile(code.ToArray()));
        }

        internal CGObject[] Run(IEnumerable<LLVM.Instruction> code)
        {
            PushFrame(new(code));

            return Run();
        }

        public CGObject[] Run()
        {
            if (frames.Count == 0)
                throw new InvalidOperationException("No frames to run.");

            Execute();

            return frames.Pop().ToArray();
        }

        private void Execute()
        {
            try
            {
                LLVM.Instruction instruction;
                while ((instruction = Frame.Instruction()).OpCode != LLVM.OpCode.End)
                {
                    Execute(instruction);
                }
            } catch (IndexOutOfRangeException)
            {
                Console.Error.WriteLine("Instruction stream did not end with a return instruction.");
            }
        }

        private void Execute(LLVM.Instruction instruction)
        {
            switch (instruction.OpCode)
            {
                case LLVM.OpCode.End:
                    throw new InvalidOperationException($"{nameof(LLVM.OpCode.End)} instruction should not be executed.");
                case LLVM.OpCode.Get:
                    if (context.Get(instruction.As<string>()) is CGObject @object)
                        Frame.Put(@object);
                    else
                        ErrorHandler.CouldNotFindName(instruction.As<string>());
                    break;
                case LLVM.OpCode.Set:
                    if (!context.Set(instruction.As<string>(), Frame.Pop()))
                        ErrorHandler.NameAlreadyExists(instruction.As<string>());
                    break;
                case LLVM.OpCode.Del:
                    if (!context.Del(instruction.As<string>()))
                        ErrorHandler.CouldNotFindName(instruction.As<string>());
                    break;
                case LLVM.OpCode.Argument:
                    Frame.Arg(instruction.As<string?>());
                    break;
                case LLVM.OpCode.Call:
                    Frame.Put(Call(instruction.As<int>()));
                    break;
                case LLVM.OpCode.GetIndex:
                    Frame.Put(GetIndex(instruction.As<int>()));
                    break;
                case LLVM.OpCode.SetIndex:
                    Frame.Put(SetIndex(instruction.As<int>()));
                    break;
                case LLVM.OpCode.DelIndex:
                    throw new NotImplementedException();
                    //break;
                case LLVM.OpCode.GetMemberName:
                    Frame.Put(GetMember(instruction.As<MemberName>()));
                    break;
                case LLVM.OpCode.SetMemberName:
                    Frame.Put(SetMember(instruction.As<MemberName>()));
                    break;
                case LLVM.OpCode.GetMemberIndex:
                    Frame.Put(GetMember(instruction.As<MemberIndex>()));
                    break;
                case LLVM.OpCode.SetMemberIndex:
                    Frame.Put(SetMember(instruction.As<MemberIndex>()));
                    break;
                case LLVM.OpCode.Assign:
                    Frame.Put(Assign());
                    break;
                case LLVM.OpCode.Literal:
                    Frame.Put(Literal(instruction.As<LLVM.Literal>()));
                    break;
                case LLVM.OpCode.Jump:
                    Frame.Jump(instruction.As<int>());
                    break;
                case LLVM.OpCode.JumpIfTrue:
                case LLVM.OpCode.JumpIfFalse:
                case LLVM.OpCode.Evaluate:
                    throw new NotImplementedException("Evaluation is not yet implemented.");
                case LLVM.OpCode.Object:
                    Frame.Put(instruction.As<CGObject>());
                    break;
                case LLVM.OpCode.Cast:
                    Frame.Put(Cast());
                    break;
                case LLVM.OpCode.Label:
                    Frame.Put(Injector.CreateInjector(() => LabelCreator(instruction.As<IR.VM.Nop>())));
                    break;
                case LLVM.OpCode.Inject:
                    Frame.Put(Injector.CreateInjector(instruction.As<HLVM.Injector>()));
                case LLVM.OpCode.Enter: context.Enter(Frame.Pop()); break;
                case LLVM.OpCode.Leave: context.Leave(); break;
                    break;
                default:
                    throw new NotImplementedException($"Unknown opcode: {instruction.OpCode}");
            }
        }

        private void PushFrame(Frame frame)
        {
            frames.Push(frame);
        }

        #region Implementations

        private Argument[] PopArgs(int argCount)
        {
            var args = new Argument[argCount];
            for (int i = argCount - 1; i >= 0; --i)
            {
                args[i] = Frame.Arg();
            }

            return args;
        }

        private CGObject Assign()
        {
            var value = Frame.Pop();
            var target = Frame.Pop();
            return CodeGenerator.Assign(target, value);
        }

        private CGObject Call(int argCount)
        {
            var args = PopArgs(argCount);
            var callee = Frame.Pop();
            return CodeGenerator.Call(callee, args);
        }

        private CGObject Cast()
        {
            var type = Frame.Pop();
            var obj = Frame.Pop();
            return CodeGenerator.Cast(obj, type);
        }

        private CGObject GetIndex(int argCount)
        {
            var args = PopArgs(argCount);
            var obj = Frame.Pop();
            return CodeGenerator.Index(obj, args);
        }

        private CGObject SetIndex(int argCount)
        {
            var args = PopArgs(argCount);
            var value = Frame.Pop();
            var obj = Frame.Pop();
            return CodeGenerator.Index(obj, args, value);
        }

        private CGObject GetMember(MemberName member)
        {
            var obj = Frame.Pop();
            return CodeGenerator.Member(obj, member);
        }

        private CGObject SetMember(MemberName member)
        {
            var value = Frame.Pop();
            var obj = Frame.Pop();
            return CodeGenerator.Member(obj, member, value);
        }

        private CGObject GetMember(MemberIndex member)
        {
            var obj = Frame.Pop();
            return CodeGenerator.Member(obj, member);
        }

        private CGObject SetMember(MemberIndex member)
        {
            var value = Frame.Pop();
            var obj = Frame.Pop();
            return CodeGenerator.Member(obj, member, value);
        }

        private CGObject Literal(LLVM.Literal literal)
        {
            return CodeGenerator.Literal(literal.Value, literal.Type, null);
        }

        private static IEnumerable<IR.VM.Instruction> LabelCreator(IR.VM.Nop label)
        {
            yield return label;
        }

        #endregion
    }
}
