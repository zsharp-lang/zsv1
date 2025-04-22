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
                IR.ConstructedClass constructedClass => LoadType(constructedClass),
                IR.OOPTypeReference reference => LoadType(reference),
                _ => throw new NotImplementedException()
            };
        }

        public Type LoadType(IR.OOPTypeReference typeReference)
        {
            var type = Context.Cache(typeReference.Definition);

            if (type is null)
                throw new();

            List<Type> genericArguments = [];

            if (typeReference.OwningType is not null)
                genericArguments.AddRange(LoadType(typeReference.OwningType).GetGenericArguments());

            if (typeReference is IR.ConstructedType constructedType)
                genericArguments.AddRange(constructedType.Arguments.Select(LoadType));

            return type.MakeGenericType([.. genericArguments]);
        }

        public Type LoadType(IR.ConstructedClass constructedClass)
        {
            var result = LoadType(constructedClass as IR.OOPTypeReference);

            return result;
            //var innerClass = LoadType(constructedClass as IR.OOPTypeReference);

            //if (innerClass is null)
            //    throw new();

            //if (constructedClass.Arguments.Count == 0)
            //    return innerClass;

            //return innerClass.MakeGenericType([
            //    .. constructedClass.Arguments.Select(LoadType)
            //]);
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

            if (!Context.Cache(@ref.Member.Method, out var def))
                throw new InvalidOperationException($"Method {@ref.Member.Name} was not loaded");

            var method = type.GetConstructor([]);

            if (method is null)
                throw new();

            if (method.HasSameMetadataDefinitionAs(def))
                return method;

            throw new();
        }

        public IL.FieldInfo LoadReference(IR.FieldReference @ref)
        {
            var type = LoadType(@ref.OwningType);

            if (!Context.Cache(@ref.Member, out var def))
                throw new InvalidOperationException($"Field {@ref.Member.Name} was not loaded");

            var field = type.GetField(def.Name);

            if (field is null)
                throw new();

            if (field.HasSameMetadataDefinitionAs(def))
                return field;

            throw new();
        }

        public IL.MethodInfo LoadReference(IR.MethodReference @ref)
        {
            var type = LoadType(@ref.OwningType);

            if (!Context.Cache(@ref.Method, out var def))
                throw new InvalidOperationException($"Method {@ref.Method.Name} was not loaded");

            var types = ((IR.ICallable)@ref).Signature.GetParameters().Select(p => p.Type).Skip(@ref.Method.IsStatic ? 0 : 1).Select(LoadType).ToArray();

            var method = type.GetMethod(
                def.Name, 
                def.GetGenericArguments().Length,
                types
            );

            if (method is null)
                throw new();

            if (@ref is IR.GenericMethodInstance genericMethodInstance)
                method = method.MakeGenericMethod([.. genericMethodInstance.Arguments.Select(LoadType)]);

            if (method.HasSameMetadataDefinitionAs(def))
                return method;

            throw new();
        }

        public IL.MethodInfo LoadReference(IR.GenericFunctionInstance @ref)
        {
            if (!Context.Cache<IL.MethodInfo>(@ref.Function, out var def))
                throw new InvalidOperationException($"Method {@ref.Function.Name} was not loaded");

            return def.MakeGenericMethod([.. @ref.Arguments.Select(LoadType)]);
        }
    }
}
