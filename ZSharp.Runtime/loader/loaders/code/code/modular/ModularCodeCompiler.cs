namespace ZSharp.Runtime.Loaders
{
    internal static partial class CodeCompiler_Impl
    {
        public static void Compile(ICodeContext ctx, IR.VM.GetGlobal get)
        {
            var global = ctx.Runtime.ImportGlobal(get.Global);

            ctx.IL.Emit(Emit.OpCodes.Ldsfld, global);

            ctx.Stack.Put(global.FieldType);
        }

        public static void Compile(ICodeContext ctx, IR.VM.SetGlobal set)
        {
            var global = ctx.Runtime.ImportGlobal(set.Global);

            ctx.IL.Emit(IL.Emit.OpCodes.Stsfld, global);

            ctx.Stack.Pop(global.FieldType);
        }
    }
}
