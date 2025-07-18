using System.Reflection;
using ZSharp.IR;

namespace ZSharp.Runtime.NET.IL2IR
{
    internal sealed class InterfaceLoader(ILLoader loader, Type input)
        : BaseILLoader<Type, Interface>(loader, input, new(input.Name))
    {
        private OOPTypeReference<Interface> Self { get; set; }

        public override Interface Load()
        {
            if (Context.Cache<Interface>(Input, out var result))
                return result;

            Context.Cache(Input, Output);

            Self = new InterfaceReference(Output);

            LoadGenericParameters();

            LoadBase();

            LoadInterfaceImplementations();

            LoadProperties();

            LoadMethods();

            LoadTypes();

            return Output;
        }

        private void LoadBase()
        {
            foreach (var @interface in Input.GetInterfaces())
                Output.Bases.Add(Loader.LoadType<OOPTypeReference<Interface>>(@interface));
        }

        private void LoadGenericParameters()
        {
            if (!Input.IsGenericTypeDefinition)
                return;

            Output.Name = Input.Name.Split('`')[0];

            foreach (var parameter in Input.GetGenericArguments())
            {
                var genericParameter = new IR.GenericParameter(parameter.Name);
                Context.Cache(parameter, genericParameter);
                Output.GenericParameters.Add(genericParameter);
            }

            Self = new ConstructedInterface(Output)
            {
                Arguments = [.. Output.GenericParameters]
            };
        }

        private void LoadInterfaceImplementations()
        {
            //foreach (var @interface in Input.GetInterfaces())
            //    LoadInterfaceImplementation(@interface, Input.GetInterfaceMap(@interface));
        }

        private void LoadProperties()
        {
            foreach (var property in Input.GetProperties())
                LoadProperty(property);
        }

        private void LoadMethods()
        {
            foreach (var method in Input.GetMethods())
                if (method.DeclaringType == Input)
                    LoadMethod(method);
        }

        private void LoadTypes()
        {
            foreach (var nested in Input.GetNestedTypes())
                LoadTypeDefinition(nested);
        }

        private void LoadInterfaceImplementation(Type @interface, IL.InterfaceMapping mapping)
        {
            if (mapping.InterfaceMethods.Length != mapping.TargetMethods.Length)
                throw new InvalidOperationException("Interface mapping is invalid.");

            //throw new NotImplementedException();

            //var implementation = new IR.InterfaceImplementation(Loader.LoadType<IR.ConstructedInterface>(@interface));

            //for (int i = 0; i < mapping.InterfaceMethods.Length; i++)
            //{
            //    var interfaceMethod = mapping.InterfaceMethods[i];
            //    var targetMethod = mapping.TargetMethods[i];

            //    implementation.Implementations.Add(
            //        LoadMethod(interfaceMethod),
            //        LoadMethod(targetMethod)
            //    );
            //}
        }

        private void LoadProperty(PropertyInfo property)
        {
            var result = new Property(property.Name, Loader.LoadType(property.PropertyType))
            {
                Getter = property.GetMethod is null ? null : LoadMethod(property.GetMethod),
                Setter = property.SetMethod is null ? null : LoadMethod(property.SetMethod),
            };

            // TODO: add property metadata
        }

        private Method LoadMethod(MethodInfo method)
        {
            var result = new Method(Loader.LoadType(method.ReturnType))
            {
                Name = method.GetCustomAttribute<AliasAttribute>()?.Name ?? method.Name,
                IsVirtual = true,
            };

            Context.Cache(method, result);

            if (!method.IsStatic)
                result.Signature.Args.Parameters.Add(new("this", Self));

            foreach (var parameter in method.GetParameters())
                if (parameter.GetCustomAttribute<KeywordParameterAttribute>() is not null)
                    result.Signature.KwArgs.Parameters.Add(new(parameter.Name ?? throw new(), Loader.LoadType(parameter.ParameterType)));
                else
                    result.Signature.Args.Parameters.Add(new(parameter.Name ?? string.Empty, Loader.LoadType(parameter.ParameterType)));

            Output.Methods.Add(result);

            return result;
        }

        private void LoadTypeDefinition(Type type)
        {
            if (!type.IsTypeDefinition)
                throw new ArgumentException("Type must be a type definition.", nameof(type));

            if (Context.Cache(type, out OOPType? result)) ;
            else if (type.IsClass) result = LoadClass(type);
            else if (type.IsInterface) result = LoadInterface(type);
            else if (type.IsEnum) result = LoadEnum(type);
            else if (type.IsValueType) result = LoadStruct(type);
            else throw new NotImplementedException();

            // TODO: add nested types
            //Output.NestedTypes.Add(result);
        }

        private Class LoadClass(Type type)
            => new ClassLoader(Loader, type).Load();

        private Interface LoadInterface(Type type)
            => new InterfaceLoader(Loader, type).Load();

        private EnumClass LoadEnum(Type type)
            => new EnumerationLoader(Loader, type).Load();

        private IR.ValueType LoadStruct(Type type)
            => new ValueTypeLoader(Loader, type).Load();
    }
}
