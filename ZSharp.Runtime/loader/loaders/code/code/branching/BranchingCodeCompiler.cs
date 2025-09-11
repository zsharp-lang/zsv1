namespace ZSharp.Runtime.Loaders
{
    internal static partial class CodeCompiler_Impl
    {
        public static void Compile(IBranchingCodeContext ctx, IR.VM.Jump jump)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Br, ctx.GetBranchTarget(jump.Target));
        }

        public static void Compile(IBranchingCodeContext ctx, IR.VM.JumpIfTrue jump)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Brtrue, ctx.GetBranchTarget(jump.Target));

            ctx.Stack.Pop(typeof(bool));
        }

        public static void Compile(IBranchingCodeContext ctx, IR.VM.JumpIfFalse jump)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Brfalse, ctx.GetBranchTarget(jump.Target));

            ctx.Stack.Pop(typeof(bool));
        }
    }
}
