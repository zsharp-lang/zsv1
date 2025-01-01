namespace ZSharp.Runtime.NET.IL2IR
{
    internal sealed class ClassLoader(ILLoader loader, Type input)
        : BaseILLoader<Type, IR.Class>(loader, input, new(input.Name))
    {
        public override IR.Class Load()
        {
            if (Context.Cache<IR.Class>(Input, out var result))
                return result;

            Context.Cache(Input, Output);

            LoadBase();

            LoadInterfaceImplementations();

            LoadFields();

            LoadProperties();

            LoadConstructors();

            LoadMethods();

            LoadTypes();

            return Output;
        }

        private void LoadBase()
        {
            if (Input.BaseType is not null)
                Output.Base = (IR.Class)Loader.LoadType(Input.BaseType);
        }

        private void LoadInterfaceImplementations()
        {
            foreach (var @interface in Input.GetInterfaces())
                LoadInterfaceImplementation(@interface, Input.GetInterfaceMap(@interface));
        }

        private void LoadFields()
        {
            foreach (var field in Input.GetFields())
                LoadField(field);
        }

        private void LoadProperties()
        {
            foreach (var property in Input.GetProperties())
                LoadProperty(property);
        }

        private void LoadConstructors()
        {
            foreach (var constructor in Input.GetConstructors())
                LoadConstructor(constructor);
        }

        private void LoadMethods()
        {
            foreach (var method in Input.GetMethods())
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

            var implementation = new IR.InterfaceImplementation(Loader.LoadType<IR.Interface>(@interface));

            for (int i = 0; i < mapping.InterfaceMethods.Length; i++)
            {
                var interfaceMethod = mapping.InterfaceMethods[i];
                var targetMethod = mapping.TargetMethods[i];

                implementation.Implementations.Add(
                    LoadMethod(interfaceMethod),
                    LoadMethod(targetMethod)
                );
            }
        }

        private void LoadField(IL.FieldInfo field)
        {
            throw new NotImplementedException();

            // TODO: implement this by adding a property, since it's impossible for the VM to access C# fields directly.
        }

        private void LoadProperty(IL.PropertyInfo property)
        {
            var result = new IR.Property(property.Name, Loader.LoadType(property.PropertyType))
            {
                Getter = property.GetMethod is null ? null : LoadMethod(property.GetMethod),
                Setter = property.SetMethod is null ? null : LoadMethod(property.SetMethod),
            };

            Output.Properties.Add(result);
        }

        private void LoadConstructor(IL.ConstructorInfo constructor)
        {
            var result = new IR.Constructor(null)
            {
                Method = new(Loader.RuntimeModule.TypeSystem.Void),
            };

            foreach (var parameter in constructor.GetParameters())
                result.Method.Signature.Args.Parameters.Add(new(parameter.Name ?? string.Empty, Loader.LoadType(parameter.ParameterType)));

            Output.Constructors.Add(result);
        }

        private IR.Method LoadMethod(IL.MethodInfo method)
        {
            var result = new IR.Method(Loader.LoadType(method.ReturnType));

            foreach (var parameter in method.GetParameters())
                result.Signature.Args.Parameters.Add(new(parameter.Name ?? string.Empty, Loader.LoadType(parameter.ParameterType)));

            Output.Methods.Add(result);

            return result;
        }

        private void LoadTypeDefinition(Type type)
        {
            if (!type.IsTypeDefinition)
                throw new ArgumentException("Type must be a type definition.", nameof(type));

            var result = Context.Cache(type);

            if (result is not null) ;
            else if (type.IsClass) result = LoadClass(type);
            else if (type.IsInterface) result = LoadInterface(type);
            else if (type.IsEnum) result = LoadEnum(type);
            else if (type.IsValueType) result= LoadStruct(type);
            else throw new NotImplementedException();

            // TADA: | | | | | | | | | 
            // TODO: V V V V V V V V V
            //Output.Types.Add(result);
        }

        private IR.Class LoadClass(Type type)
        {
            throw new NotImplementedException();
        }

        private IR.Interface LoadInterface(Type type)
        {
            throw new NotImplementedException();
        }

        private IR.Enumclass LoadEnum(Type type)
        {
            throw new NotImplementedException();
        }

        private IR.ValueType LoadStruct(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
