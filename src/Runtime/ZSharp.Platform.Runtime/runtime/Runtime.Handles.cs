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

        internal void EmitCall(Emit.ILGenerator il, LIR.ICallable callable)
        {
            switch (callable)
            {
                case LIR.Function function:
                    il.Emit(Emit.OpCodes.Call, ImportFunction(function));
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
