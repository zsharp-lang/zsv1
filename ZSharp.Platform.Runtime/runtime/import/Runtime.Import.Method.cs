namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        public IL.MethodInfo ImportMethod(IR.Method method)
            => ImportFunction(method.UnderlyingFunction);

        public IL.MethodInfo ImportMethodReference(IR.MethodReference @ref)
        {
            var type = ImportType(@ref.OwningType);

            var def = ImportMethod(@ref.Method);

            if (type.IsGenericType)
                if (type.GetGenericTypeDefinition() is IL.Emit.TypeBuilder typeBuilder)
                {
                    if (!typeBuilder.IsCreated())
                        def = Emit.TypeBuilder.GetMethod(type, def);
                }
                else
                    try
                    {
                        def = (IL.MethodInfo)(IL.MethodBase.GetMethodFromHandle(
                            def.MethodHandle,
                            type.TypeHandle
                        ) ?? throw new("Could not create method from method handle"));
                    }
                    catch (NotSupportedException)
                    {
                        def = Emit.TypeBuilder.GetMethod(type, def);
                    }

            if (@ref is IR.ConstructedMethod constructed)
                def = def.MakeGenericMethod([
                    .. constructed.Arguments.Select(ImportType)
                ]);

            return def;
        }
    }
}
