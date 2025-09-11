namespace ZSharp.Runtime.NET.IR2IL.Code
{
    internal static partial class CodeCompiler_Impl
    {
        public static void Compile(ICodeContext ctx, IR.VM.GetGlobal get)
        {
            if (!ctx.Loader.Context.Cache(get.Global, out var global))
                throw new();

            ctx.IL.Emit(IL.Emit.OpCodes.Ldsfld, global);

            ctx.Stack.Put(global.FieldType);
        }

        public static void Compile(ICodeContext ctx, IR.VM.SetGlobal set)
        {
            if (!ctx.Loader.Context.Cache(set.Global, out var global))
                throw new();

            ctx.IL.Emit(IL.Emit.OpCodes.Stsfld, global);

            ctx.Stack.Pop(global.FieldType);
        }
    }
}
