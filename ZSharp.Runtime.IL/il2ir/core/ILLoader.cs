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
                if (type.GenericTypeArguments.Length != 0)
                    throw new InvalidOperationException();

                OOPType def;

                if (type.IsClass) def = new ClassLoader(this, type).Load();
                else if (type.IsInterface) def = new InterfaceLoader(this, type).Load();
                else if (type.IsEnum) def = new EnumerationLoader(this, type).Load();
                else if (type.IsValueType) def = new ValueTypeLoader(this, type).Load();
                else throw new ArgumentException("Type must be a type definition.", nameof(type));

                return def switch
                {
                    Class @class => new ClassReference(@class),
                    Interface @interface => new InterfaceReference(@interface),
                    EnumClass @enum => @enum,
                    IR.ValueType valueType => new ValueTypeReference(valueType),
                    _ => throw new NotSupportedException()
                };
            }

            if (type.IsArray)
                return LoadArrayType(type);
            if (type.IsPointer)
                return LoadPointerType(type);
            if (type.IsByRef)
                return LoadReferenceType(type);

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
                    Interface @interface => new ConstructedInterface(@interface)
                    {
                        Arguments = [.. genericArguments],
                    },
                    IR.ValueType valueType => new ConstructedValueType(valueType)
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

        public Module LoadModule(IL.Module module)
        {
            return new ModuleLoader(this, module).Load();
        }

        private IType LoadArrayType(Type type)
        {
            var elementType = LoadType(type.GetElementType()!);

            return new ConstructedClass(
                RuntimeModule.TypeSystem.Array
            ) {
                Arguments = [
                    elementType
                ]
            };
        }

        private IType LoadPointerType(Type type)
        {
            var elementType = LoadType(type.GetElementType()!);

            return new ConstructedClass(
                RuntimeModule.TypeSystem.Pointer
            )
            {
                Arguments = [
                    elementType
                ]
            };
        }

        private IType LoadReferenceType(Type type)
        {
            var elementType = LoadType(type.GetElementType()!);

            return new ConstructedClass(
                RuntimeModule.TypeSystem.Reference
            )
            {
                Arguments = [
                    elementType
                ]
            };
        }
    }
}
