using System.Text.RegularExpressions;

namespace ZSharp.Compiler.IRLoader
{
    public partial class IRLoader
    {
        public Context Context { get; } = new();

        public partial Module Import(ZSharp.IR.Module module)
            => Import(module, new(module.Name!)
            {
                IR = module,
            });

        public CompilerObject Import(ZSharp.IR.IType type)
            => Load(type);

        public CompilerObject Import(ZSharp.IR.Function function)
            => Context.Objects.Cache(function)!;

        private Module Import(ZSharp.IR.Module module, Module result)
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

            var ops = Compiler.Feature<Ops>();

            if (module.HasFunctions)
                foreach (var function in module.Functions)
                {
                    if (function.Name is null || function.Name == string.Empty)
                        continue;

                    var match = Regex.Match(function.Name, @"^_?(?<OP>[+\-*/=?&^%$#@!<>|~]+)_?$");
                    if (match.Success)
                    {
                        var op = match.Groups["OP"].Value;
                        if (!ops.Binary.Cache(op, out var group))
                            group = ops.Binary.Cache(op, new OverloadGroup(op));

                        if (group is not OverloadGroup overloadGroup)
                            throw new Exception("Invalid overload group!");

                        overloadGroup.Overloads.Add(Import(function));
                    }
                }

            return result;
        }

        private Action Load(ZSharp.IR.Module module, Module owner)
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

        private Action Load(ZSharp.IR.Function function, Module owner)
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

        private Action Load(ZSharp.IR.OOPType type, Module owner)
            => type switch
            {
                ZSharp.IR.Class @class => Load(@class, owner),
                ZSharp.IR.Interface @interface => Load(@interface, owner),
                //ZSharp.IR.Struct @struct => Load(@struct),
                _ => throw new NotImplementedException(),
            };

        private Action Load(ZSharp.IR.Class @class, Module owner)
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
                result.Base = (IClass)Load(@class.Base);

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
                        CompilerObject resultMethod;
                        if (method.HasGenericParameters)
                            resultMethod = LoadGenericMethod(method, result);
                        else
                        {
                            resultMethod = new Method(method.Name)
                            {
                                IR = method,
                                Defined = true,
                                Owner = result,
                                Signature = Load(method.Signature)
                            };
                        }
                        if (method.Name is not null && method.Name != string.Empty)
                        {
                            if (!result.Members.TryGetValue(method.Name, out var group))
                                result.Members.Add(method.Name, group = new OverloadGroup(method.Name));

                            if (group is not OverloadGroup overloadGroup)
                                throw new InvalidOperationException();

                            overloadGroup.Overloads.Add(resultMethod);
                        }
                    }
            };
        }

        private GenericMethod LoadGenericMethod(ZSharp.IR.Method method, CompilerObject owner)
        {
            GenericMethod result = new(method.Name)
            {
                Defined = true,
                IR = method,
                Owner = owner
            };

            foreach (var genericParameter in method.GenericParameters)
                result.GenericParameters.Add(
                    Context.Types.Cache(genericParameter, new GenericParameter()
                    {
                        IR = genericParameter,
                        Name = genericParameter.Name
                    })
                );

            result.Signature = Load(method.Signature);

            return result;
        }

        private Action Load(ZSharp.IR.Interface @interface, Module owner)
        {
            Interface result = new(@interface.Name)
            {
                Name = @interface.Name ?? string.Empty,
                IR = @interface,
                IsDefined = true,
            };

            Context.Objects.Cache(@interface, result);

            owner.Content.Add(result);

            if (result.Name is not null && result.Name != string.Empty)
                owner.Members.Add(result.Name, result);

            //if (@interface.HasGenericParameters)
            //    foreach (var parameter in @interface.GenericParameters)
            //    {
            //        var genericParameter = new GenericParameter()
            //        {
            //            Name = parameter.Name,
            //            IR = parameter,
            //        };

            //        Context.Types.Cache(parameter, genericParameter);

            //        result.GenericParameters.Add(genericParameter);
            //    }

            if (@interface.HasBases)
                foreach (var @base in @interface.Bases)
                    result.Bases.Add(Load(@base));

            return () =>
            {
                if (@interface.HasMethods)
                    foreach (var method in @interface.Methods)
                    {
                        Method resultMethod = new(method.Name)
                        {
                            IR = method,
                            Defined = true,
                            Owner = result
                        };
                        
                        result.Content.Add(resultMethod);
                        
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

        private IType Load(ZSharp.IR.IType type)
        {
            if (Context.Types.Cache(type, out var result))
                return result;

            return type switch
            {
                ZSharp.IR.ClassReference classReference => Load(classReference),
                ZSharp.IR.ConstructedClass constructedClass => Load(constructedClass),
                ZSharp.IR.InterfaceReference interfaceReference => Load(interfaceReference),
                _ => null!
            };
        }

        private IType Load(ZSharp.IR.ClassReference classReference)
            => Context.Objects.Cache<Class>(classReference.Definition) ?? throw new();

        private IType Load(ZSharp.IR.ConstructedClass constructed)
        {

            if (Context.Objects.Cache<GenericClass>(constructed.Class, out var genericClass))
            {
                var args = new CommonZ.Utils.Cache<CompilerObject, CompilerObject>();

                if (genericClass.GenericParameters.Count != constructed.Arguments.Count)
                    throw new();

                for (var i = 0; i < constructed.Arguments.Count; i++)
                    args.Cache(genericClass.GenericParameters[i], Import(constructed.Arguments[i]));

                return new GenericClassInstance(genericClass, new()
                {
                    Scope = genericClass,
                    CompileTimeValues = args
                });
            }

            return Context.Types.Cache(constructed) ?? throw new();
        }

        private IType Load(ZSharp.IR.InterfaceReference interfaceReference)
            => Context.Objects.Cache<Interface>(interfaceReference.Definition) ?? throw new();

        private Action LoadGenericClass(ZSharp.IR.Class @class, Module owner)
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
                result.Base = (IClass)Load(@class.Base);

            var self = new GenericClassInstance(result, new()
            {
                Scope = result,
                CompileTimeValues = new()
            });

            if (@class.HasGenericParameters)
                foreach (var parameter in @class.GenericParameters)
                {
                    var genericParameter = new GenericParameter()
                    {
                        Name = parameter.Name,
                        IR = parameter,
                    };

                    Context.Types.Cache(parameter, genericParameter);
                    self.Context[genericParameter] = genericParameter;

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
                            Owner = self,
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
                            Owner = result
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
                    }
            };
        }

        private Action LoadGenericFunction(ZSharp.IR.Function function, Module owner)
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
            };
        }

        private Signature Load(ZSharp.IR.Signature signature)
        {
            Signature result = new()
            {
                ReturnType = Load(signature.ReturnType)
            };

            if (signature.HasArgs)
                foreach (var arg in signature.Args.Parameters)
                    result.Args.Add(LoadParameter(arg));

            if (signature.IsVarArgs)
                result.VarArgs = LoadVarParameter(signature.Args.Var!);

            if (signature.HasKwArgs)
                foreach (var arg in signature.KwArgs.Parameters)
                    result.KwArgs.Add(LoadParameter(arg));

            if (signature.IsVarKwArgs)
                result.VarKwArgs = LoadVarParameter(signature.KwArgs.Var!);

            return result;
        }

        private Parameter LoadParameter(ZSharp.IR.Parameter parameter)
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

        private VarParameter LoadVarParameter(ZSharp.IR.Parameter parameter)
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
    }
}
