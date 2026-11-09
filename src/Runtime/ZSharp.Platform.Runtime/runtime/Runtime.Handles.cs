namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<LIR.TypeHandle, Type> typeHandles = [];
        private readonly Dictionary<LIR.FunctionHandle, Action<Emit.ILGenerator>> functionHandles = [];

        /// <summary>
        /// Handles of instance methods of value types, which take their object by its address.
        /// </summary>
        private readonly HashSet<LIR.FunctionHandle> byAddress = [];

        /// <summary>
        /// The CLR methods handles are mapped to, which a member of a generic instance is
        /// found from.
        /// </summary>
        private readonly Dictionary<LIR.FunctionHandle, IL.MethodBase> methodHandles = [];

        /// <summary>
        /// Whether a call passes its first argument by its address, as an instance method of
        /// a value type takes its object.
        /// </summary>
        internal bool TakesAddress(LIR.ICallable callable)
            => callable switch
            {
                LIR.FunctionHandle handle => byAddress.Contains(handle),
                LIR.InstanceMember member => TakesAddress(member.Member),
                _ => false
            };

        public void Map(LIR.TypeHandle handle, Type type)
            => typeHandles[handle] = type;

        public void Map(LIR.FunctionHandle handle, IL.MethodBase method)
        {
            if (!method.IsStatic && method is IL.MethodInfo && method.DeclaringType is { IsValueType: true })
                byAddress.Add(handle);

            methodHandles[handle] = method;
            Map(handle, il => EmitCall(il, method));
        }

        /// <summary>
        /// Maps a handle to code that is emitted in place of a call. The arguments
        /// are already on the stack.
        /// </summary>
        public void Map(LIR.FunctionHandle handle, Action<Emit.ILGenerator> emit)
            => functionHandles[handle] = emit;

        /// <summary>
        /// Maps a handle to code that casts a value down to a type it might be of, which
        /// fails if it isn't. The type may be one the runtime is still building, so it is
        /// found as the code is emitted.
        /// </summary>
        public void MapDowncast(LIR.FunctionHandle handle, LIR.Type type)
            => Map(handle, il =>
            {
                var imported = ImportType(type);

                il.Emit(imported.IsValueType ? Emit.OpCodes.Unbox_Any : Emit.OpCodes.Castclass, imported);
            });

        /// <summary>
        /// Maps a handle to code that tells whether a value is of a type, which may be one
        /// the runtime is still building, so it is found as the code is emitted.
        /// </summary>
        public void MapTypeTest(LIR.FunctionHandle handle, LIR.Type type)
            => Map(handle, il =>
            {
                il.Emit(Emit.OpCodes.Isinst, ImportType(type));
                il.Emit(Emit.OpCodes.Ldnull);
                il.Emit(Emit.OpCodes.Cgt_Un);
            });

        /// <summary>
        /// Maps a handle to code that makes a delegate of a method. Its argument is the
        /// object the method is bound to, or null for a static method. A virtual method is
        /// bound through the object, so the delegate calls what the object overrides.
        /// </summary>
        public void MapDelegate(LIR.FunctionHandle handle, Type delegateType, IL.MethodInfo method)
            => Map(handle, il => EmitDelegate(il, delegateType, method));

        /// <summary>
        /// Maps a handle to code that makes a delegate of a method of a class the runtime is
        /// still building, which is found as the code is emitted.
        /// </summary>
        public void MapDelegate(LIR.FunctionHandle handle, Type delegateType, ClassMethod method)
            => Map(handle, il => EmitDelegate(il, delegateType, Referenced(method)));

        private static void EmitDelegate(Emit.ILGenerator il, Type delegateType, IL.MethodInfo method)
        {
            if (method.IsVirtual && !method.IsStatic)
            {
                il.Emit(Emit.OpCodes.Dup);
                il.Emit(Emit.OpCodes.Ldvirtftn, method);
            }
            else
                il.Emit(Emit.OpCodes.Ldftn, method);

            il.Emit(Emit.OpCodes.Newobj, delegateType.GetConstructor([typeof(object), typeof(nint)])!);
        }

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
                case LIR.Function function when TryMethod(function, out var method):
                    EmitCall(il, method);
                    break;
                case LIR.Function function:
                    il.Emit(Emit.OpCodes.Call, ImportFunction(function));
                    break;
                case ClassConstructor constructor:
                    il.Emit(Emit.OpCodes.Newobj, Referenced(constructor));
                    break;
                case ClassMethod method:
                    il.Emit(method.IsStatic ? Emit.OpCodes.Call : Emit.OpCodes.Callvirt, Referenced(method));
                    break;
                case ClassFieldAccess access:
                    il.Emit(access.IsSet ? Emit.OpCodes.Stfld : Emit.OpCodes.Ldfld, Referenced(access.Field));
                    break;
                case LIR.InstanceMember member:
                    EmitCall(il, OnInstance(member));
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

        /// <summary>
        /// A member of a generic definition as an instance of it has it. While a type
        /// argument is still being built, the instance has no members of its own to ask
        /// for, so the member is found through the definition's.
        /// </summary>
        private IL.MethodBase OnInstance(LIR.InstanceMember member)
        {
            if (member.Member is not LIR.FunctionHandle handle || !methodHandles.TryGetValue(handle, out var method))
                throw new NotSupportedException("A member of a generic instance is a method of its definition.");

            var type = ImportType(member.Owner);

            return ContainsBuilder(type)
                ? method switch
                {
                    IL.ConstructorInfo constructor => Emit.TypeBuilder.GetConstructor(type, constructor),
                    IL.MethodInfo info => Emit.TypeBuilder.GetMethod(type, info),
                    _ => throw new NotSupportedException($"Can't call a {method.GetType().Name}.")
                }
                : IL.MethodBase.GetMethodFromHandle(method.MethodHandle, type.TypeHandle)!;
        }

        private static void EmitCall(Emit.ILGenerator il, IL.MethodBase method)
        {
            switch (method)
            {
                case IL.ConstructorInfo constructor:
                    il.Emit(Emit.OpCodes.Newobj, constructor);
                    break;
                // A value type's own method is called directly, on the address of its object.
                case IL.MethodInfo info:
                    il.Emit(info.IsStatic || info.DeclaringType is { IsValueType: true } ? Emit.OpCodes.Call : Emit.OpCodes.Callvirt, info);
                    break;
                default:
                    throw new NotSupportedException($"Can't call a {method.GetType().Name}.");
            }
        }
    }
}
