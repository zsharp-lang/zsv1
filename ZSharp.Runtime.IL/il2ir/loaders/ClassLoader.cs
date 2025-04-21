using CommonZ.Utils;
using System.Reflection;
using ZSharp.IR;

namespace ZSharp.Runtime.NET.IL2IR
{
    internal sealed class ClassLoader(ILLoader loader, Type input)
        : BaseILLoader<Type, IR.Class>(loader, input, new(input.Name))
    {
        private IR.OOPTypeReference<IR.Class> Self { get; set; }

        public override IR.Class Load()
        {
            if (Context.Cache<IR.Class>(Input, out var result))
                return result;

            Context.Cache(Input, Output);

            Self = new IR.ClassReference(Output);

            LoadGenericParameters();

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
                Output.Base = (OOPTypeReference<Class>)Loader.LoadType(Input.BaseType);
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

            Self = new IR.ConstructedClass(Output)
            {
                Arguments = [.. Output.GenericParameters]
            };
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

        private void LoadField(IL.FieldInfo field)
        {
            var result = new IR.Field(field.Name, Loader.LoadType(field.FieldType))
            {
                IsStatic = field.IsStatic,
                IsReadOnly = field.IsInitOnly,
            };

            Output.Fields.Add(result);
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

            Context.Cache(constructor, result.Method);

            if (!constructor.IsStatic)
                result.Method.Signature.Args.Parameters.Add(new("this", Self));

            foreach (var parameter in constructor.GetParameters())
                result.Method.Signature.Args.Parameters.Add(new(parameter.Name ?? string.Empty, Loader.LoadType(parameter.ParameterType)));

            Output.Constructors.Add(result);
        }

        private IR.Method LoadMethod(IL.MethodInfo method)
        {
            var result = new IR.Method(Loader.LoadType(method.ReturnType))
            {
                Name = method.GetCustomAttribute<AliasAttribute>()?.Name ?? method.Name,
            };

            Context.Cache(method, result);

            if (!method.IsStatic)
                result.Signature.Args.Parameters.Add(new("this", Self));

            foreach (var parameter in method.GetParameters())
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

            Output.NestedTypes.Add(result);
        }

        private IR.Class LoadClass(Type type)
            => new ClassLoader(Loader, type).Load();

        private IR.Interface LoadInterface(Type type)
        {
            return new();
            throw new NotImplementedException();
        }

        private IR.Enumclass LoadEnum(Type type)
        {
            return new();
            throw new NotImplementedException();
        }

        private IR.ValueType LoadStruct(Type type)
        {
            return new();
            throw new NotImplementedException();
        }
    }
}
