namespace ZSharp.Platform.Runtime.Loaders
{
    internal static partial class CodeCompiler_Impl
    {
        public static void Compile(IFrameCodeContext ctx, IR.VM.GetArgument get)
        {
            var parameter = ctx.GetParameter(get.Argument);
            var index = parameter.Index;

            if (index == 0)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldarg_0);
            else if (index == 1)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldarg_1);
            else if (index == 2)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldarg_2);
            else if (index == 3)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldarg_3);
            else if (index <= byte.MaxValue)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldarg_S, (byte)index);
            else
                ctx.IL.Emit(IL.Emit.OpCodes.Ldarg, index);

            ctx.Stack.Put(parameter.Type);
        }

        public static void Compile(IFrameCodeContext ctx, IR.VM.GetLocal get)
        {
            var l = ctx.GetLocal(get.Local);
            var index = l.Index;

            if (index == 0)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldloc_0);
            else if (index == 1)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldloc_1);
            else if (index == 2)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldloc_2);
            else if (index == 3)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldloc_3);
            else if (index <= byte.MaxValue)
                ctx.IL.Emit(IL.Emit.OpCodes.Ldloc_S, (byte)index);
            else
                ctx.IL.Emit(IL.Emit.OpCodes.Ldloc, index);

            ctx.Stack.Put(l.Type);
        }

        public static void Compile(IFrameCodeContext ctx, IR.VM.SetArgument set)
        {
            var parameter = ctx.GetParameter(set.Argument);
            var index = parameter.Index;

            if (index <= byte.MaxValue)
                ctx.IL.Emit(IL.Emit.OpCodes.Starg_S, (byte)index);
            else
                ctx.IL.Emit(IL.Emit.OpCodes.Starg, index);

            ctx.Stack.Pop(parameter.Type);
        }

        public static void Compile(IFrameCodeContext ctx, IR.VM.SetLocal set)
        {
            var local = ctx.GetLocal(set.Local);
            var index = local.Index;

            if (index <= byte.MaxValue)
                ctx.IL.Emit(IL.Emit.OpCodes.Stloc_S, (byte)index);
            else
                ctx.IL.Emit(IL.Emit.OpCodes.Stloc, index);

            ctx.Stack.Pop(local.Type);
        }
    }
}
