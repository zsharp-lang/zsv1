using ZSharp.IR;

namespace ZSharp.Runtime.NET.IL2IR
{
    public sealed class ILLoader(Context context, RuntimeModule? runtimeModule = null)
    {
        public RuntimeModule RuntimeModule { get; } = runtimeModule ?? RuntimeModule.Standard;

        public Context Context { get; } = context;

        public IType LoadType(Type type)
        {
            if (Context.Cache(type, out var result))
                return result;

            if (type.IsTypeDefinition)
            {
                var genericArguments = type.GenericTypeArguments.Select(LoadType).ToList();

                var @class = new ClassLoader(this, type).Load();

                if (genericArguments.Count == 0) result = new ClassReference(@class);
                else if (type.IsClass) result = new ConstructedClass(@class)
                {
                    Arguments = [.. genericArguments],
                };
                //else if (type.IsInterface) result = new ILInterfaceLoader(this, type).Load();
                //else if (type.IsEnum) result = new ILEnumLoader(this, type).Load();
                //else if (type.IsValueType) result = new ILStructLoader(this, type).Load();
                else throw new ArgumentException("Type must be a type definition.", nameof(type));

                return result;
            }

            if (type.IsArray)
                throw new NotImplementedException();
            if (type.IsPointer)
                throw new NotImplementedException();
            if (type.IsByRef)
                throw new NotImplementedException();

            if (type.IsConstructedGenericType)
            {
                var definition = type.GetGenericTypeDefinition();

                var definitionIR = LoadType(definition);

                var genericArguments = type.GenericTypeArguments.Select(LoadType).ToList();

                if (definitionIR is not OOPTypeReference typeReference)
                    throw new ArgumentException("Type must be a type definition.", nameof(type));

                return typeReference.Definition switch
                {
                    Class @class => new ConstructedClass(@class)
                    {
                        Arguments = [.. genericArguments],
                    },
                    _ => throw new NotImplementedException()
                };
            }

            throw new();
        }

        public T LoadType<T>(Type type)
            where T : IType
            => (T)LoadType(type);

        public IR.Module LoadModule(IL.Module module)
        {
            return new ModuleLoader(this, module).Load();
        }
    }
}
