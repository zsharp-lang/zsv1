namespace ZSharp.Runtime.Loaders
{
    internal static partial class CodeCompiler_Impl
    {
        public static void Compile(ICodeContext ctx, IR.VM.Call call)
        {
            var callable = ctx.Runtime.ImportCallable(call.Callable);

            foreach (var parameter in call.Callable.Signature.GetParameters())
                ctx.Stack.Pop(ctx.Runtime.ImportType(parameter.Type));

            if (callable is IL.MethodInfo method)
            {
                ctx.IL.Emit(Emit.OpCodes.Call, method);

                if (method.ReturnType != typeof(void))
                    ctx.Stack.Put(method.ReturnType);
            }
            else if (callable is IL.ConstructorInfo constructor)
                ctx.IL.Emit(Emit.OpCodes.Call, constructor);
            else throw new($"Unknown callable type: {callable.GetType()}");
        }

        public static void Compile(ICodeContext ctx, IR.VM.CallIndirect call)
        {
            throw new NotImplementedException();
        }

        public static void Compile(ICodeContext ctx, IR.VM.Return _)
        {
            ctx.IL.Emit(Emit.OpCodes.Ret);
        }
    }
}
