namespace ZSharp.Compiler.IRLoader
{
    public partial class IRLoader
    {
        public Context Context { get; } = new();

        public partial Module Import(IR.Module module)
            => Import(module, new(module.Name!)
            {
                IR = module,
            });

        public CompilerObject Import(IR.IType type)
            => Load(type);

        public CompilerObject Import(IR.Function function)
            => Context.Objects.Cache(function)!;

        private Module Import(IR.Module module, Module result)
        {
            result ??= new(module.Name!)
            {
                IR = module,
            };

            List<Action> actions = [];

            if (module.HasSubmodules)
                foreach (var submodule in module.Submodules)
                    actions.Add(Load(submodule, result));

            if (module.HasFunctions)
                foreach (var function in module.Functions)
                    actions.Add(Load(function, result));

            foreach (var action in actions)
                action();

            return result;
        }

        private Action Load(IR.Module module, Module owner)
        {
            Module result = new(module.Name!)
            {
                IR = module,
            };

            Context.Objects.Cache(module, result);

            owner.Content.Add(result);

            if (result.Name is not null && result.Name != string.Empty)
                owner.Members.Add(result.Name, result);

            return () => Import(module, result);
        }

        private Action Load(IR.Function function, Module owner)
        {
            RTFunction result = new(function.Name)
            {
                IR = function
            };

            Context.Objects.Cache(function, result);

            owner.Content.Add(result);

            if (result.Name != string.Empty)
            {
                if (!owner.Members.TryGetValue(result.Name, out var group))
                    owner.Members.Add(result.Name, group = new OverloadGroup(result.Name));

                if (group is not OverloadGroup overloadGroup)
                    throw new InvalidOperationException();

                overloadGroup.Overloads.Add(result);
            }

            return () =>
            {
                result.Signature = Load(function.Signature);

                result.ReturnType = Load(function.ReturnType);
            };
        }

        private Action Load(IR.OOPType type)
            => type switch
            {
                IR.Class @class => Load(@class),
                //IR.Interface @interface => Load(@interface),
                //IR.Struct @struct => Load(@struct),
                _ => throw new NotImplementedException(),
            };

        private Action Load(IR.Class @class)
        {
            throw new NotImplementedException();
        }

        private CompilerObject Load(IR.IType type)
        {
            if (Context.Types.Cache(type, out var result))
                return result;

            throw new NotImplementedException();
        }

        private Signature Load(IR.Signature signature)
        {
            Signature result = new();

            if (signature.HasArgs)
                foreach (var arg in signature.Args.Parameters)
                    result.Args.Add(Load(arg));

            if (signature.IsVarArgs)
                result.VarArgs = Load(signature.Args.Var!);

            if (signature.HasKwArgs)
                foreach (var arg in signature.KwArgs.Parameters)
                    result.KwArgs.Add(Load(arg));

            if (signature.IsVarKwArgs)
                result.VarKwArgs = Load(signature.KwArgs.Var!);

            return result;
        }

        private Parameter Load(IR.Parameter parameter)
        {
            var type = Load(parameter.Type);

            return new(parameter.Name)
            {
                IR = parameter,
                Initializer = parameter.Initializer is null ? null : new RawCode(new(parameter.Initializer)
                {
                    Types = [type]
                }),
                Type = type,
            };
        }
    }
}
