namespace ZSharp.Runtime.NET.IR2IL.Code
{
    internal static partial class CodeCompiler_Impl
    {
        public static void Compile(ICodeContext ctx, IR.VM.Call call)
        {
            var callable = ctx.Loader.LoadReference(call.Callable);

            foreach (var parameter in call.Callable.Signature.GetParameters())
                ctx.Stack.Pop(ctx.Loader.LoadType(parameter.Type));

            if (callable is IL.MethodInfo method)
            {
                ctx.IL.Emit(IL.Emit.OpCodes.Call, method);

                if (method.ReturnType != typeof(void))
                    ctx.Stack.Put(method.ReturnType);
            }
            else if (callable is IL.ConstructorInfo constructor)
                ctx.IL.Emit(IL.Emit.OpCodes.Call, constructor);
            else throw new($"Unknown callable type: {callable.GetType()}");
        }

        public static void Compile(ICodeContext ctx, IR.VM.CallIndirect call)
        {
            throw new NotImplementedException();
        }

        public static void Compile(ICodeContext ctx, IR.VM.Return _)
        {
            ctx.IL.Emit(IL.Emit.OpCodes.Ret);
        }
    }
}
