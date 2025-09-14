namespace ZSharp.Platform.Runtime.Loaders
{
    internal static partial class CodeCompiler_Impl
    {
        public static void Compile(ICodeContext ctx, IR.VM.Dup _)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Dup);

            ctx.Stack.Dup();
        }

        public static void Compile(ICodeContext ctx, IR.VM.IsNotNull _)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Ldnull);
            ctx.IL.Emit(IL.Emit.OpCodes.Cgt_Un);

            ctx.Stack.Pop();
            ctx.Stack.Put(typeof(bool));
        }

        public static void Compile(ICodeContext ctx, IR.VM.IsNull _)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Ldnull);
            ctx.IL.Emit(IL.Emit.OpCodes.Ceq);

            ctx.Stack.Pop();
            ctx.Stack.Put(typeof(bool));
        }

        public static void Compile(ICodeContext ctx, IR.VM.Nop _)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Nop);
        }

        public static void Compile(ICodeContext ctx, IR.VM.Pop _)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Pop);

            ctx.Stack.Pop();
        }

        public static void Compile(ICodeContext ctx, IR.VM.PutBoolean put)
        {
            ctx.IL.Emit(put.Value ? IL.Emit.OpCodes.Ldc_I4_1 : IL.Emit.OpCodes.Ldc_I4_0);

            ctx.Stack.Put(typeof(bool));
        }

        public static void Compile(ICodeContext ctx, IR.VM.PutFloat32 put)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Ldc_R4, put.Value);

            ctx.Stack.Put(typeof(float));
        }

        public static void Compile(ICodeContext ctx, IR.VM.PutInt32 put)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Ldc_I4, put.Value);

            ctx.Stack.Put(typeof(int));
        }

        public static void Compile(ICodeContext ctx, IR.VM.PutNull _)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Ldnull);

            ctx.Stack.Put(typeof(object));
        }

        public static void Compile(ICodeContext ctx, IR.VM.PutString put)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Ldstr, put.Value);

            ctx.Stack.Put(typeof(string));
        }

        public static void Compile(ICodeContext ctx, IR.VM.Swap _)
        {
            throw new NotImplementedException();
        }
    }
}
