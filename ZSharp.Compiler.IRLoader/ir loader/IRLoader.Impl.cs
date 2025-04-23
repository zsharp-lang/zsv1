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

            if (module.HasTypes)
                foreach (var type in module.Types)
                    actions.Add(Load(type, result));

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
            if (function.HasGenericParameters)
                return LoadGenericFunction(function, owner);

            RTFunction result = new(function.Name)
            {
                IR = function,
                IsDefined = true,
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

        private Action Load(IR.OOPType type, Module owner)
            => type switch
            {
                IR.Class @class => Load(@class, owner),
                //IR.Interface @interface => Load(@interface),
                //IR.Struct @struct => Load(@struct),
                _ => throw new NotImplementedException(),
            };

        private Action Load(IR.Class @class, Module owner)
        {
            if (@class.HasGenericParameters)
                return LoadGenericClass(@class, owner);

            Class result = new()
            {
                Name = @class.Name ?? string.Empty,
                IR = @class,
                IsDefined = true,
            };

            Context.Objects.Cache(@class, result);

            owner.Content.Add(result);

            if (result.Name is not null && result.Name != string.Empty)
                owner.Members.Add(result.Name, result);

            if (@class.Base is not null)
                result.Base = Load(@class.Base);

            return () =>
            {
                if (@class.HasFields)
                    foreach (var field in @class.Fields)
                    {
                        Field resultField = new(field.Name)
                        {
                            IR = field,
                            Type = Load(field.Type),
                        };
                        if (field.Initializer is not null)
                            resultField.Initializer = new RawCode(new(field.Initializer)
                            {
                                Types = [resultField.Type]
                            });
                        result.Members.Add(resultField.Name, resultField);
                    };

                if (@class.Constructors.Count > 0)
                    foreach (var constructor in @class.Constructors)
                    {
                        Constructor resultConstructor = new(constructor.Name)
                        {
                            Owner = result,
                            IR = constructor,
                        };
                        if (resultConstructor.Name is not null && resultConstructor.Name != string.Empty)
                        {
                            if (!result.Members.TryGetValue(resultConstructor.Name, out var group))
                                result.Members.Add(resultConstructor.Name, group = new OverloadGroup(resultConstructor.Name));

                            if (group is not OverloadGroup overloadGroup)
                                throw new InvalidOperationException();

                            overloadGroup.Overloads.Add(resultConstructor);
                        } else
                        {
                            if (result.Constructor is not OverloadGroup group)
                                result.Constructor = group = new OverloadGroup(string.Empty);

                            group.Overloads.Add(resultConstructor);
                        }
                        resultConstructor.Signature = Load(constructor.Method.Signature);
                    }

                if (@class.Methods.Count > 0)
                    foreach (var method in @class.Methods)
                    {
                        Method resultMethod = new(method.Name)
                        {
                            IR = method,
                            Defined = true,
                        };
                        if (resultMethod.Name is not null && resultMethod.Name != string.Empty)
                        {
                            if (!result.Members.TryGetValue(resultMethod.Name, out var group))
                                result.Members.Add(resultMethod.Name, group = new OverloadGroup(resultMethod.Name));

                            if (group is not OverloadGroup overloadGroup)
                                throw new InvalidOperationException();

                            overloadGroup.Overloads.Add(resultMethod);
                        }
                        resultMethod.Signature = Load(method.Signature);
                        resultMethod.ReturnType = Load(method.ReturnType);
                    }
            };
        }

        private CompilerObject Load(IR.IType type)
        {
            if (Context.Types.Cache(type, out var result))
                return result;

            if (type is IR.ConstructedClass constructed)
                return Load(constructed);

            return null!;
            //throw new NotImplementedException();
        }

        private CompilerObject Load(IR.ConstructedClass constructed)
        {
            var origin = 
                Context.Objects.Cache<GenericClass>(constructed.Class) ??
                Context.Types.Cache(constructed)
                ?? throw new();

            var args = new CommonZ.Utils.Cache<CompilerObject, CompilerObject>();

            if (origin is GenericClass genericClass)
            {
                if (genericClass.GenericParameters.Count != constructed.Arguments.Count)
                    throw new();

                for (var i = 0; i < constructed.Arguments.Count; i++)
                    args.Cache(genericClass.GenericParameters[i], Import(constructed.Arguments[i]));

                return new GenericClassInstance(genericClass)
                {
                    Context = new()
                    {
                        Scope = genericClass,
                        CompileTimeValues = args
                    }
                };
            }

            return origin;
        }

        private Action LoadGenericClass(IR.Class @class, Module owner)
        {
            GenericClass result = new()
            {
                Name = @class.Name ?? string.Empty,
                IR = new(@class),
                Defined = true,
            };

            Context.Objects.Cache(@class, result);

            owner.Content.Add(result);

            if (result.Name is not null && result.Name != string.Empty)
                owner.Members.Add(result.Name, result);

            if (@class.Base is not null)
                result.Base = Load(@class.Base);

            if (@class.HasGenericParameters)
                foreach (var parameter in @class.GenericParameters)
                {
                    var genericParameter = new GenericParameter()
                    {
                        Name = parameter.Name,
                        IR = parameter,
                    };

                    Context.Types.Cache(parameter, genericParameter);

                    result.GenericParameters.Add(genericParameter);
                }

            return () =>
            {
                if (@class.HasFields)
                    foreach (var field in @class.Fields)
                    {
                        Field resultField = new(field.Name)
                        {
                            IR = field,
                            Type = Load(field.Type),
                        };
                        if (field.Initializer is not null)
                            resultField.Initializer = new RawCode(new(field.Initializer)
                            {
                                Types = [resultField.Type]
                            });
                        result.Members.Add(resultField.Name, resultField);
                    }
                ;

                if (@class.Constructors.Count > 0)
                    foreach (var constructor in @class.Constructors)
                    {
                        Constructor resultConstructor = new(constructor.Name)
                        {
                            Owner = result,
                            IR = constructor,
                        };
                        if (resultConstructor.Name is not null && resultConstructor.Name != string.Empty)
                        {
                            if (!result.Members.TryGetValue(resultConstructor.Name, out var group))
                                result.Members.Add(resultConstructor.Name, group = new OverloadGroup(resultConstructor.Name));

                            if (group is not OverloadGroup overloadGroup)
                                throw new InvalidOperationException();

                            overloadGroup.Overloads.Add(resultConstructor);
                        }
                        else
                        {
                            if (result.Constructor is not OverloadGroup group)
                                result.Constructor = group = new OverloadGroup(string.Empty);

                            group.Overloads.Add(resultConstructor);
                        }
                        resultConstructor.Signature = Load(constructor.Method.Signature);
                    }

                if (@class.Methods.Count > 0)
                    foreach (var method in @class.Methods)
                    {
                        Method resultMethod = new(method.Name)
                        {
                            IR = method,
                            Defined = true,
                        };
                        if (resultMethod.Name is not null && resultMethod.Name != string.Empty)
                        {
                            if (!result.Members.TryGetValue(resultMethod.Name, out var group))
                                result.Members.Add(resultMethod.Name, group = new OverloadGroup(resultMethod.Name));

                            if (group is not OverloadGroup overloadGroup)
                                throw new InvalidOperationException();

                            overloadGroup.Overloads.Add(resultMethod);
                        }
                        resultMethod.Signature = Load(method.Signature);
                        resultMethod.ReturnType = Load(method.ReturnType);
                    }
            };
        }

        private Action LoadGenericFunction(IR.Function function, Module owner)
        {
            GenericFunction result = new(function.Name)
            {
                IR = function,
                Defined = true,
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
                foreach (var parameter in function.GenericParameters)
                {
                    var genericParameter = new GenericParameter()
                    {
                        Name = parameter.Name,
                        IR = parameter,
                    };

                    Context.Types.Cache(parameter, genericParameter);

                    result.GenericParameters.Add(genericParameter);
                }

                    result.Signature = Load(function.Signature);

                result.ReturnType = Load(function.ReturnType);
            };
        }

        private Signature Load(IR.Signature signature)
        {
            Signature result = new();

            if (signature.HasArgs)
                foreach (var arg in signature.Args.Parameters)
                    result.Args.Add(LoadParameter(arg));

            if (signature.IsVarArgs)
                result.VarArgs = LoadVarParameter(signature.Args.Var!);

            if (signature.HasKwArgs)
                foreach (var arg in signature.KwArgs.Parameters)
                    result.KwArgs.Add(LoadParameter(arg));

            if (signature.IsVarKwArgs)
                result.VarKwArgs = LoadKeywordVarParameter(signature.KwArgs.Var!);

            return result;
        }

        private Parameter LoadParameter(IR.Parameter parameter)
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

        private VarParameter LoadVarParameter(IR.Parameter parameter)
        {
            var type = Load(parameter.Type);

            if (parameter.Initializer is not null)
                throw new NotImplementedException();

            return new(parameter.Name)
            {
                IR = parameter,
                Type = type,
            };
        }

        private KeywordVarParameter LoadKeywordVarParameter(IR.Parameter parameter)
        {
            var type = Load(parameter.Type);

            return new(parameter.Name)
            {
                IR = parameter,
                Type = type,
            };
        }
    }
}
