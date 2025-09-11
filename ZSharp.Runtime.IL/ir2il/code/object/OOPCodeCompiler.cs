namespace ZSharp.Runtime.NET.IR2IL.Code
{
    internal static partial class CodeCompiler_Impl
    {
        public static void Compile(ICodeContext ctx, IR.VM.CallVirtual callVirtual)
        {
            var method = ctx.Loader.LoadReference(callVirtual.Method);

            if (!method.IsVirtual && !method.IsAbstract)
                throw new($"Method {method} is not virtual or abstract!");

            ctx.IL.Emit(IL.Emit.OpCodes.Callvirt, method);

            if (method.ReturnType != typeof(void))
                ctx.Stack.Put(method.ReturnType);
        }

        public static void Compile(ICodeContext ctx, IR.VM.CastReference castReference)
        {
            var targetType = ctx.Loader.LoadType(castReference.Type);

            ctx.IL.Emit(IL.Emit.OpCodes.Isinst, targetType);

            ctx.Stack.Pop();
            ctx.Stack.Put(targetType);
        }

        public static void Compile(ICodeContext ctx, IR.VM.CreateInstance createInstance)
        {
            var constructor = ctx.Loader.LoadReference(createInstance.Constructor);

            ctx.IL.Emit(IL.Emit.OpCodes.Newobj, constructor);

            ctx.Stack.Put(constructor.DeclaringType ?? throw new());
        }

        public static void Compile(ICodeContext ctx, IR.VM.GetField get)
        {
            var field = ctx.Loader.LoadReference(get.Field);

            ctx.IL.Emit(field.IsStatic ? IL.Emit.OpCodes.Ldsfld : IL.Emit.OpCodes.Ldfld, field);

            if (!field.IsStatic)
                ctx.Stack.Pop(field.DeclaringType ?? throw new());
            ctx.Stack.Put(field.FieldType);
        }

        public static void Compile(ICodeContext ctx, IR.VM.SetField set)
        {
            var field = ctx.Loader.LoadReference(set.Field);

            ctx.IL.Emit(field.IsStatic ? IL.Emit.OpCodes.Stsfld : IL.Emit.OpCodes.Stfld, field);

            ctx.Stack.Pop();
            if (!field.IsStatic)
                ctx.Stack.Pop(field.DeclaringType ?? throw new());
        }
    }
}
