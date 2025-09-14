namespace ZSharp.Platform.Runtime.Loaders
{
    internal static partial class CodeCompiler_Impl
    {
        public static void Compile(ICodeContext ctx, IR.VM.CallVirtual callVirtual)
        {
            var method = ctx.Runtime.ImportMethodReference(callVirtual.Method);

            if (!method.IsVirtual && !method.IsAbstract)
                throw new($"Method {method} is not virtual or abstract!");

            ctx.IL.Emit(Emit.OpCodes.Callvirt, method);

            if (method.ReturnType != typeof(void))
                ctx.Stack.Put(method.ReturnType);
        }

        public static void Compile(ICodeContext ctx, IR.VM.CastReference castReference)
        {
            var targetType = ctx.Runtime.ImportType(castReference.Type);

            ctx.IL.Emit(Emit.OpCodes.Isinst, targetType);

            ctx.Stack.Pop();
            ctx.Stack.Put(targetType);
        }

        public static void Compile(ICodeContext ctx, IR.VM.CreateInstance createInstance)
        {
            var constructor = ctx.Runtime.ImportConstructorReference(createInstance.Constructor);

            ctx.IL.Emit(Emit.OpCodes.Newobj, constructor);

            ctx.Stack.Put(constructor.DeclaringType ?? throw new());
        }

        public static void Compile(ICodeContext ctx, IR.VM.GetField get)
        {
            var field = ctx.Runtime.ImportFieldReference(get.Field);

            ctx.IL.Emit(field.IsStatic ? Emit.OpCodes.Ldsfld : Emit.OpCodes.Ldfld, field);

            if (!field.IsStatic)
                ctx.Stack.Pop(field.DeclaringType ?? throw new());
            ctx.Stack.Put(field.FieldType);
        }

        public static void Compile(ICodeContext ctx, IR.VM.SetField set)
        {
            var field = ctx.Runtime.ImportFieldReference(set.Field);

            ctx.IL.Emit(field.IsStatic ? Emit.OpCodes.Stsfld : Emit.OpCodes.Stfld, field);

            ctx.Stack.Pop();
            if (!field.IsStatic)
                ctx.Stack.Pop(field.DeclaringType ?? throw new());
        }
    }
}
