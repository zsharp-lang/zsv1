using ZSharp.CGRuntime.LLVM;

namespace ZSharp.CGRuntime
{
    internal static class JIT
    {
        public static Instruction[] Compile(HLVM.Instruction[] instructions)
        {
            var code = instructions.Select(Compile).ToArray();

            var labels = new Dictionary<Instruction, int>();
            
            for (int i = 0; i < code.Length; ++i)
            {
                ref var instruction = ref code[i];
                if (instruction.As<HLVM.Instruction>() is HLVM.Instruction target)
                {
                    if (!labels.TryGetValue(instruction, out var index))
                        // do note that instructions is mapped 1-to-1 with code so this works
                        index = labels[instruction] = Array.IndexOf(instructions, instruction);
                    instruction.Operand = index;
                }
            }

            return code;
        }

        private static Instruction Compile(HLVM.Instruction instruction)
            => instruction switch
            {
                // bindings
                HLVM.Binding binding => binding.AccessMode switch
                {
                    HLVM.AccessMode.Get => new(OpCode.Get, binding.Name),
                    HLVM.AccessMode.Set => new(OpCode.Set, binding.Name),
                    HLVM.AccessMode.Del => new(OpCode.Del, binding.Name),
                    _ => throw new Exception($"Unknown access mode: {binding.AccessMode}")
                },

                // callables
                HLVM.Argument argument => new(OpCode.Argument, argument.Name),
                HLVM.Call call => new(OpCode.Call, call.ArgumentCount),

                // index
                HLVM.Index index => index.AccessMode switch
                {
                    HLVM.AccessMode.Get => new(OpCode.GetIndex),
                    HLVM.AccessMode.Set => new(OpCode.SetIndex),
                    HLVM.AccessMode.Del => new(OpCode.DelIndex),
                    _ => throw new Exception($"Unknown access mode: {index.AccessMode}")
                },

                // members
                HLVM.Member<MemberName> member => member.AccessMode switch
                {
                    HLVM.AccessMode.Get => new(OpCode.GetMemberName, member.MemberPosition),
                    HLVM.AccessMode.Set => new(OpCode.SetMemberName, member.MemberPosition),
                    HLVM.AccessMode.Del => throw new NotImplementedException("Delete member is not implemented"),
                    _ => throw new Exception($"Unknown access mode: {member.AccessMode}")
                },
                HLVM.Member<MemberIndex> member => member.AccessMode switch
                {
                    HLVM.AccessMode.Get => new(OpCode.GetMemberIndex, member.MemberPosition),
                    HLVM.AccessMode.Set => new(OpCode.SetMemberIndex, member.MemberPosition),
                    HLVM.AccessMode.Del => throw new NotImplementedException("Delete member is not implemented"),
                    _ => throw new Exception($"Unknown access mode: {member.AccessMode}")
                },

                // assign
                HLVM.Assign => new(OpCode.Assign),

                // literals
                HLVM.Literal literal => new(OpCode.Literal, new Literal(literal.Value, literal.Type)),

                // evaluate
                HLVM.Evaluate => new(OpCode.Evaluate),

                // this
                HLVM.Object @object => new(OpCode.Object, @object.CGObject),

                // cast
                HLVM.Cast => new(OpCode.Cast),

                // return
                HLVM.Return => new(OpCode.Ret),

                // flow
                HLVM.Jump jump => jump.Condition switch
                {
                    HLVM.JumpCondition.Always => new(OpCode.Jump, jump.Target),
                    HLVM.JumpCondition.IfTrue => new(OpCode.JumpIfTrue, jump.Target),
                    HLVM.JumpCondition.IfFalse => new(OpCode.JumpIfFalse, jump.Target),
                    _ => throw new Exception($"Unknown condition: {jump.Condition}")
                },

                // label
                HLVM.Label label => new(OpCode.Label, label.Target),

                // inject
                HLVM.Inject inject => new(OpCode.Inject, inject.Injector),

                _ => throw new Exception($"Unknown instruction type: {instruction.GetType().Name}")
            };
    }
}
