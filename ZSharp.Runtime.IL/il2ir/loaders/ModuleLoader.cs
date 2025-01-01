using static System.Reflection.CustomAttributeExtensions;

namespace ZSharp.Runtime.NET.IL2IR
{
    internal sealed class ModuleLoader(ILLoader loader, IL.Module input)
        : BaseILLoader<IL.Module, IR.Module>(loader, input, new(input.Name))
    {
        public override IR.Module Load()
        {
            if (Input.GetCustomAttribute<AliasAttribute>() is AliasAttribute alias)
                Output.Name = alias.Name;

            if (Context.Cache(Input, out var result))
                return result;
            Context.Cache(Input, Output);

            LoadFields();

            LoadMethods();

            LoadTypes();

            return Output;
        }

        private void LoadFields()
        {
            foreach (var field in Input.GetFields())
                LoadField(field);
        }

        private void LoadMethods()
        {
            foreach (var method in Input.GetMethods())
                LoadMethod(method);
        }

        private void LoadTypes()
        {
            foreach (var type in Input.GetTypes())
                if (type.GetCustomAttribute<HideInIRAttribute>() is null)
                    LoadTypeDefinition(type);
        }

        private void LoadField(IL.FieldInfo field)
        {
            var global = new IR.Global(field.Name, Loader.LoadType(field.FieldType))
            {
                //IsReadOnly = field.IsInitOnly,
            };

            Output.Globals.Add(global);
        }

        private void LoadMethod(IL.MethodInfo method)
        {
            var function = new IR.Function(Loader.LoadType(method.ReturnType))
            {
                Name = method.GetCustomAttribute<AliasAttribute>() is AliasAttribute alias ? alias.Name : method.Name,
            };

            foreach (var parameter in method.GetParameters())
                function.Signature.Args.Parameters.Add(new(parameter.Name!, Loader.LoadType(parameter.ParameterType)));

            Output.Functions.Add(function);

            Context.Cache(function, method);
        }

        private void LoadTypeDefinition(Type type)
        {
            if (!type.IsTypeDefinition)
                throw new ArgumentException("Type must be a type definition.", nameof(type));

            if (type.IsClass && type.IsSealed && type.IsAbstract && type.GetCustomAttribute<ModuleGlobalsAttribute>() is not null)
                LoadGlobals(type);

            else if (type.IsClass) LoadClass(type);
            else if (type.IsInterface) LoadInterface(type);
            else if (type.IsEnum) LoadEnum(type);
            else if (type.IsValueType) LoadStruct(type);

            else throw new NotImplementedException();
        }

        private void LoadGlobals(Type type)
        {
            foreach (var method in type.GetMethods())
                if (method.IsStatic)
                    LoadMethod(method);
        }

        private void LoadClass(Type type)
            => Output.Types.Add(new ClassLoader(Loader, type).Load());

        private void LoadInterface(Type type)
        {
            throw new NotImplementedException();
        }

        private void LoadEnum(Type type)
        {
            throw new NotImplementedException();
        }

        private void LoadStruct(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
