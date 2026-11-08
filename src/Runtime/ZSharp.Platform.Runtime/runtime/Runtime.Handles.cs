namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<LIR.TypeHandle, Type> typeHandles = [];
        private readonly Dictionary<LIR.FunctionHandle, Action<Emit.ILGenerator>> functionHandles = [];

        public void Map(LIR.TypeHandle handle, Type type)
            => typeHandles[handle] = type;

        public void Map(LIR.FunctionHandle handle, IL.MethodBase method)
            => Map(handle, il => EmitCall(il, method));

        /// <summary>
        /// Maps a handle to code that is emitted in place of a call. The arguments
        /// are already on the stack.
        /// </summary>
        public void Map(LIR.FunctionHandle handle, Action<Emit.ILGenerator> emit)
            => functionHandles[handle] = emit;

        /// <summary>
        /// Maps a handle to code that makes a delegate of a method. Its argument is the
        /// object the method is bound to, or null for a static method. A virtual method is
        /// bound through the object, so the delegate calls what the object overrides.
        /// </summary>
        public void MapDelegate(LIR.FunctionHandle handle, Type delegateType, IL.MethodInfo method)
            => Map(handle, il =>
            {
                if (method.IsVirtual && !method.IsStatic)
                {
                    il.Emit(Emit.OpCodes.Dup);
                    il.Emit(Emit.OpCodes.Ldvirtftn, method);
                }
                else
                    il.Emit(Emit.OpCodes.Ldftn, method);

                il.Emit(Emit.OpCodes.Newobj, delegateType.GetConstructor([typeof(object), typeof(nint)])!);
            });

        /// <summary>
        /// Maps a handle to code that gives the type object of a type, which works for a
        /// type that is still being defined too.
        /// </summary>
        public void MapTypeOf(LIR.FunctionHandle handle, LIR.Type type)
            => Map(handle, il =>
            {
                il.Emit(Emit.OpCodes.Ldtoken, ImportType(type));
                il.Emit(Emit.OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!);
            });

        /// <summary>
        /// Maps a handle to code that reads an element of an array of objects as a value
        /// of a type. Its arguments are the array and the index.
        /// </summary>
        public void MapGetElement(LIR.FunctionHandle handle, LIR.Type type)
            => Map(handle, il =>
            {
                var imported = ImportType(type);

                il.Emit(Emit.OpCodes.Ldelem_Ref);
                il.Emit(imported.IsValueType ? Emit.OpCodes.Unbox_Any : Emit.OpCodes.Castclass, imported);
            });

        /// <summary>
        /// Maps a handle to code that writes a value of a type to an element of an array
        /// of objects. Its arguments are the array, the index and the value.
        /// </summary>
        public void MapSetElement(LIR.FunctionHandle handle, LIR.Type type)
            => Map(handle, il =>
            {
                var imported = ImportType(type);

                if (imported.IsValueType)
                    il.Emit(Emit.OpCodes.Box, imported);

                il.Emit(Emit.OpCodes.Stelem_Ref);
            });

        internal void EmitCall(Emit.ILGenerator il, LIR.ICallable callable)
        {
            switch (callable)
            {
                case LIR.Function function:
                    il.Emit(Emit.OpCodes.Call, ImportFunction(function));
                    break;
                case ClassConstructor constructor:
                    il.Emit(Emit.OpCodes.Newobj, classConstructors[constructor]);
                    break;
                case ClassMethod method:
                    il.Emit(method.IsStatic ? Emit.OpCodes.Call : Emit.OpCodes.Callvirt, classMethods[method]);
                    break;
                case ClassFieldAccess access:
                    il.Emit(access.IsSet ? Emit.OpCodes.Stfld : Emit.OpCodes.Ldfld, classFields[access.Field]);
                    break;
                case LIR.FunctionHandle handle:
                    if (!functionHandles.TryGetValue(handle, out var emit))
                        throw new InvalidOperationException("A function handle is not mapped.");
                    emit(il);
                    break;
                default:
                    throw new NotSupportedException($"Can't call a {callable.GetType().Name}.");
            }
        }

        private static void EmitCall(Emit.ILGenerator il, IL.MethodBase method)
        {
            switch (method)
            {
                case IL.ConstructorInfo constructor:
                    il.Emit(Emit.OpCodes.Newobj, constructor);
                    break;
                case IL.MethodInfo info:
                    il.Emit(info.IsStatic ? Emit.OpCodes.Call : Emit.OpCodes.Callvirt, info);
                    break;
                default:
                    throw new NotSupportedException($"Can't call a {method.GetType().Name}.");
            }
        }
    }
}
