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
                if (type.IsClass) result = new ClassLoader(this, type).Load();
                //else if (type.IsInterface) result = new ILInterfaceLoader(this, type).Load();
                //else if (type.IsEnum) result = new ILEnumLoader(this, type).Load();
                //else if (type.IsValueType) result = new ILStructLoader(this, type).Load();
                else throw new ArgumentException("Type must be a type definition.", nameof(type));

                return Context.Cache(type, result);
            }

            if (type.IsArray)
                throw new NotImplementedException();
            if (type.IsPointer)
                throw new NotImplementedException();
            if (type.IsByRef)
                throw new NotImplementedException();

            if (type.IsConstructedGenericType)
                throw new NotImplementedException();

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
