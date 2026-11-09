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
                ClrMethod method => method.TakesAddress,
                LIR.InstanceMember member => TakesAddress(member.Member),
                LIR.MethodInstance instance => TakesAddress(instance.Method),
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

        internal void EmitCall(Emit.ILGenerator il, LIR.ICallable callable)
        {
            switch (callable)
            {
                case LIR.Function function when IsOperation(function):
                    EmitOperation(il, function);
                    break;
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
                // A field of a class defined here, on an instance of the class.
                case LIR.InstanceMember { Member: ClassFieldAccess access } member:
                    il.Emit(access.IsSet ? Emit.OpCodes.Stfld : Emit.OpCodes.Ldfld, FieldOnInstance(access.Field, ImportType(member.Owner)));
                    break;
                case LIR.InstanceMember member:
                    EmitCall(il, OnInstance(member));
                    break;
                case LIR.MethodInstance instance:
                    EmitCall(il, Instantiated(instance));
                    break;
                case ClrMethod method:
                    EmitCall(il, ImportClrMethod(method));
                    break;
                case ClrCode code:
                    EmitClrCode(il, code);
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
            => OnInstance(Defined(member.Member) ?? throw new NotSupportedException("A member of a generic instance is a method of its definition."), ImportType(member.Owner));

        /// <summary>
        /// The CLR method a handle stands for, as its declaring type defines it.
        /// </summary>
        private IL.MethodBase? Defined(LIR.ICallable callable)
            => callable switch
            {
                ClrMethod method => ImportClrMethod(method),
                LIR.FunctionHandle handle when methodHandles.TryGetValue(handle, out var method) => method,
                ClassMethod method => Declared(method),
                ClassConstructor constructor => Declared(constructor),
                LIR.InstanceMember member => OnInstance(member),
                _ => null
            };

        /// <summary>
        /// A generic method's instance, made as the code is emitted, so its type arguments
        /// may be types the runtime is still building.
        /// </summary>
        private IL.MethodInfo Instantiated(LIR.MethodInstance instance)
        {
            if (Defined(instance.Method) is not IL.MethodInfo { IsGenericMethodDefinition: true } generic)
                throw new NotSupportedException("A method instance is of a generic method.");

            return generic.MakeGenericMethod([.. instance.Arguments.Select(ImportType)]);
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
