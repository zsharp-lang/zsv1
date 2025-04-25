namespace ZSharp.Runtime.NET.IR2IL
{
    /// <summary>
    /// Wraps an IR module in a C# module.
    /// </summary>
    public class IRLoader(Context context, IR.RuntimeModule? runtimeModule = null)
    {
        public Context Context { get; } = context;

        public IR.RuntimeModule RuntimeModule { get; } = runtimeModule ?? IR.RuntimeModule.Standard;

        public Action<ICodeLoader, IR.VM.GetObject> GetObjectFunction { get; set; } = (_, __) => throw new NotImplementedException();

        public IL.Module LoadModule(IR.Module module)
        {
            if (Context.Cache(module, out var result))
                return result;

             result = new RootModuleLoader(this, module).Load();

            if (module.Initializer is not null)
                result
                    .GetType(Constants.GlobalsTypeName)!
                    .GetMethod(Context.Cache(module.Initializer)!.Name, [])!
                    .Invoke(null, null);

            return result;
        }

        public Type LoadType(IR.IType type)
        {
            if (Context.Cache(type, out var result))
                return result;

            return type switch
            {
                IR.OOPTypeReference reference => LoadTypeReference(reference),
                _ => throw new NotImplementedException()
            };
        }

        public Type LoadTypeReference(IR.OOPTypeReference typeReference)
        {
            var type = Context.Cache(typeReference.Definition);

            if (type is null)
                throw new();

            List<Type> genericArguments = [];

            if (typeReference.OwningType is not null)
                genericArguments.AddRange(LoadType(typeReference.OwningType).GetGenericArguments());

            if (typeReference is IR.ConstructedType constructedType)
                genericArguments.AddRange(constructedType.Arguments.Select(LoadType));

            if (genericArguments.Count == 0)
                return type;

            return type.MakeGenericType([.. genericArguments]);
        }

        public IL.MethodBase LoadReference(IR.ICallable callable)
        {
            if (Context.Cache(callable) is IL.MethodBase result)
                return result;

            if (callable is IR.GenericFunctionInstance genericFunctionInstance)
                return LoadReference(genericFunctionInstance);

            if (callable is IR.MethodReference methodReference)
                return LoadReference(methodReference);

            throw new NotImplementedException();
        }

        public IL.ConstructorInfo LoadReference(IR.ConstructorReference @ref)
        {
            var type = LoadType(@ref.OwningType);

            if (!Context.Cache(@ref.Constructor.Method, out var def))
                throw new InvalidOperationException($"Constructor {@ref.Constructor.Name ?? "<Anonymous>"} was not loaded");

            if (def is not IL.ConstructorInfo constructorInfo)
                throw new InvalidOperationException($"Method {@ref.Constructor.Name ?? "<Anonymous>"} was not compiled to an IL method");

            if (!type.IsGenericType)
                return constructorInfo;

            if (type.GetGenericTypeDefinition() is IL.Emit.TypeBuilder typeBuilder)
                if (!typeBuilder.IsCreated())
                    return IL.Emit.TypeBuilder.GetConstructor(type, constructorInfo);

            return (IL.ConstructorInfo)(IL.MethodBase.GetMethodFromHandle(
                constructorInfo.MethodHandle,
                type.TypeHandle
            ) ?? throw new("Could not create constructor from method handle"));
        }

        public IL.FieldInfo LoadReference(IR.FieldReference @ref)
        {
            var type = LoadType(@ref.OwningType);

            if (!Context.Cache(@ref.Member, out var def))
                throw new InvalidOperationException($"Field {@ref.Member.Name} was not loaded");

            if (!type.IsGenericType)
                return def;

            if (type.GetGenericTypeDefinition() is IL.Emit.TypeBuilder typeBuilder)
                if (!typeBuilder.IsCreated())
                    return IL.Emit.TypeBuilder.GetField(type, def);

            return IL.FieldInfo.GetFieldFromHandle(
                def.FieldHandle,
                type.TypeHandle
            );
        }

        public IL.MethodInfo LoadReference(IR.MethodReference @ref)
        {
            var type = LoadType(@ref.OwningType);

            if (!Context.Cache(@ref.Method, out var def))
                throw new InvalidOperationException($"Method {@ref.Method.Name} was not loaded");

            if (def is not IL.MethodInfo methodInfo)
                throw new InvalidOperationException($"Method {@ref.Method.Name} was not compiled to an IL method");

            if (!type.IsGenericType)
                return methodInfo;

            if (type.GetGenericTypeDefinition() is IL.Emit.TypeBuilder typeBuilder)
                if (!typeBuilder.IsCreated())
                    return IL.Emit.TypeBuilder.GetMethod(type, methodInfo);

            return (IL.MethodInfo)(IL.MethodBase.GetMethodFromHandle(
                methodInfo.MethodHandle,
                type.TypeHandle
            ) ?? throw new("Could not create method from method handle"));
        }

        public IL.MethodInfo LoadReference(IR.GenericFunctionInstance @ref)
        {
            if (!Context.Cache<IL.MethodInfo>(@ref.Function, out var def))
                throw new InvalidOperationException($"Method {@ref.Function.Name} was not loaded");

            return def.MakeGenericMethod([.. @ref.Arguments.Select(LoadType)]);
        }
    }
}
