using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// The CLR's definitions written in Z#: classes, interfaces and modules. Each is a
    /// definition, as a Z# definition is, lowered for each CLR platform it is used on, to
    /// LIR that platform realizes, rather than a type of the running process: the same
    /// class is the runtime's class and a class of an assembly written for a version of
    /// .NET.
    /// <para>
    /// Lowering one for a target declares it, its members and the types of their
    /// signatures, and then lowers its code, once whatever it reached is declared too, so
    /// definitions that reach each other, through their code or their signatures, are
    /// lowered together: the first one asked for lowers the code of all of them.
    /// </para>
    /// </summary>
    partial class ILLoader
    {
        private readonly Dictionary<Package.DotNet.Class, Objects.DefinedClass> defined = [];

        /// <summary>
        /// The code still to lower for each target, and the targets whose code is being
        /// lowered, by the first definition asked for.
        /// </summary>
        private readonly Dictionary<object, Queue<Func<Error?>>> pending = [];
        private readonly HashSet<object> draining = [];

        /// <summary>
        /// The parameters of each constructor and method of a defined class, its object first,
        /// as it was declared for a target, which its code is lowered with later.
        /// </summary>
        private readonly Dictionary<(object Member, object Target), List<LIR.Parameter>> signatures = [];

        /// <summary>
        /// The class each nested class is nested in, known once that class is loaded.
        /// </summary>
        private readonly Dictionary<Package.DotNet.Class, Package.DotNet.Class> nestedIn = new(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// What stands for no target, for code lowered for none.
        /// </summary>
        private static readonly object NoTarget = new();

        /// <summary>
        /// The description of a type the runtime made of a class defined in Z#: the class's,
        /// or none, if it isn't one.
        /// </summary>
        private Package.DotNet.TypeReference? DefinedAs(Type type)
        {
            if (runtime.DefinitionOf(type) is not { } definition)
                return null;

            foreach (var @class in defined.Values)
                if (@class.Lowerings.TryGetValue(runtime, out var lowered) && lowered.Definition == definition)
                    return Description(@class.Node);

            return null;
        }

        /// <summary>
        /// What describes a class defined in Z#: an interface, for the class-shaped view an
        /// interface is defined through.
        /// </summary>
        internal Package.DotNet.TypeReference Description(Package.DotNet.Class node)
            => viewed.TryGetValue(node, out var @interface) ? @interface : node;

        /// <summary>
        /// Lowers a definition for a target, if it is one of the CLR's defined in Z#, as what
        /// runs code where it is defined lowers it for itself: a document defines what it
        /// runs with. It gives why it can't be lowered, if it can't.
        /// </summary>
        public Error? Define(HIR.Instance node, object target)
            => node switch
            {
                Package.DotNet.Class @class => Defined(@class).When(out var loaded).Error(out var error)
                    ? error
                    : Lower((Objects.DefinedClass)loaded!, target).When(out _).Error(out error) ? error : null,
                Package.DotNet.Interface @interface => Define(View(@interface), target),
                Package.DotNet.Module module => DefineModule(module).When(out var loaded).Error(out var error)
                    ? error
                    : Lower((Objects.DefinedModule)loaded!, target).When(out _).Error(out error) ? error : null,
                _ => null
            };

        /// <summary>
        /// What type parameters are given while members of a generic instance are typed,
        /// innermost last.
        /// </summary>
        private readonly List<Dictionary<Package.DotNet.GenericParameter, CompilerObject>> substitutions = [];

        internal T Substituting<T>(Dictionary<Package.DotNet.GenericParameter, CompilerObject> given, Func<T> typing)
        {
            substitutions.Add(given);

            try
            {
                return typing();
            }
            finally
            {
                substitutions.RemoveAt(substitutions.Count - 1);
            }
        }

        private CompilerObject? Substituted(Package.DotNet.GenericParameter parameter)
        {
            for (var index = substitutions.Count - 1; index >= 0; index--)
                if (substitutions[index].TryGetValue(parameter, out var given))
                    return given;

            return null;
        }

        /// <summary>
        /// The description of an instance of a generic class defined in Z#, as code that
        /// builds it from what describes each argument.
        /// </summary>
        internal IResult? DescribeInstance(Package.DotNet.TypeReference definition, IEnumerable<CompilerObject> arguments)
        {
            var description = Class(typeof(ZSharp.HIR.Type));
            var described = new List<Argument>();

            foreach (var argument in arguments)
                if (Scope.CG.ImplicitCast(argument, description).When(out var converted).Error(out _))
                    return null;
                else
                    described.Add(new Argument(converted!));

            return Scope.CG.Call(
                new Objects.Method(this, typeof(Package.DotNet.GenericInstance).GetMethod(nameof(Package.DotNet.GenericInstance.Of))!, null),
                [new Argument(new Objects.Literal(definition, typeof(Package.DotNet.TypeReference), this)), .. described]
            );
        }

        /// <summary>
        /// What a definition of the CLR's defined in Z# was lowered to for a target: a class
        /// definition, an LIR module, or nothing, if it is none or isn't lowered for it.
        /// </summary>
        public object? Lowered(HIR.Instance node, object target)
            => node switch
            {
                Package.DotNet.Class @class when defined.TryGetValue(@class, out var known) && known.Lowerings.TryGetValue(target, out var lowered) => lowered.Definition,
                Package.DotNet.Interface @interface => Lowered(View(@interface), target),
                Package.DotNet.Module module when definedModules.TryGetValue(module, out var known) && known.Lowerings.TryGetValue(target, out var lowered) => lowered.Module,
                _ => null
            };

        /// <summary>
        /// The CO of a class defined in Z#, which is checked once, as it is first loaded.
        /// </summary>
        private IResult Defined(Package.DotNet.Class node)
        {
            if (defined.TryGetValue(node, out var known))
                return Result.Ok(known);

            if (Check(node) is { } error)
                return Result.Error(error);

            var @class = new Objects.DefinedClass(this, node);
            defined[node] = @class;

            foreach (var nested in node.NestedDefinitions.OfType<Package.DotNet.Class>())
                nestedIn[nested] = node;

            foreach (var parameter in node.GenericParameters.Concat(node.Methods.SelectMany(method => method.GenericParameters)))
                typeParameters[parameter] = new(parameter, target => @class.For(target).When(out var lowered).Error(out var failure)
                    ? Result<LIR.Type>.Error(failure)
                    : Result<LIR.Type>.Ok(lowered!.Parameters[parameter]));

            return Result.Ok(@class);
        }

        /// <summary>
        /// Lowers what a target needs, then the code of what it reached, if no lowering for
        /// the target is doing that already.
        /// </summary>
        private IResult<T, Error> Lowering<T>(object? target, Func<object, IResult<T, Error>> declare)
            where T : class
        {
            var key = target ?? NoTarget;
            var outermost = draining.Add(key);

            try
            {
                if (declare(key).When(out var declared).Error(out var error))
                    return Result<T>.Error(error);

                if (outermost && pending.TryGetValue(key, out var queue))
                    while (queue.TryDequeue(out var lower))
                        if (lower() is { } failed)
                        {
                            queue.Clear();
                            return Result<T>.Error(failed);
                        }

                return Result<T>.Ok(declared!);
            }
            finally
            {
                if (outermost)
                    draining.Remove(key);
            }
        }

        private void Later(object key, Func<Error?> lower)
        {
            if (!pending.TryGetValue(key, out var queue))
                pending[key] = queue = new();

            queue.Enqueue(lower);
        }

        /// <summary>
        /// A class as a target has it: a class definition of the target's, its members
        /// declared, and its code lowered with whatever it reached.
        /// </summary>
        internal IResult<Objects.LoweredClass, Error> Lower(Objects.DefinedClass @class, object? target)
        {
            if (@class.Lowerings.TryGetValue(target ?? NoTarget, out var known))
                return Result<Objects.LoweredClass>.Ok(known);

            if (target is not LIR.CLR.ClrPlatform)
                return Result<Objects.LoweredClass>.Error($"Class {@class.Name} can only be compiled for a CLR platform.");

            return Lowering(target, key =>
            {
                var node = @class.Node;
                var lowered = new Objects.LoweredClass(new LIR.CLR.ClassDefinition(node.Name));

                // An interface's accessibility is the interface's, not the view's it is defined by.
                lowered.Definition.Metadata.Add(Lowered((Description(node) as Package.DotNet.Accessible ?? node).Accessibility));

                // What is said of the class to keep is kept on what it lowers to: of an
                // interface, what is said of the interface, not of the view it is defined by.
                foreach (var annotation in Description(node) is HIR.Annotated said ? said.Annotations : node.Annotations)
                    if (annotation is HIR.Kept)
                        lowered.Definition.Metadata.Add(new LIR.KeptAnnotation(annotation));

                // A class's type parameters exist before its members are declared, which use them.
                foreach (var parameter in node.GenericParameters)
                {
                    var declared = new LIR.CLR.ClassGenericParameter(parameter.Name);

                    lowered.Definition.GenericParameters.Add(declared);
                    lowered.Parameters[parameter] = declared;
                }

                @class.Lowerings[key] = lowered;

                // A nested class is defined with the class it is nested in, which is lowered
                // first.
                if (nestedIn.TryGetValue(node, out var outerNode) && defined.TryGetValue(outerNode, out var outer))
                {
                    if (Lower(outer, target).When(out var enclosing).Error(out var failure))
                    {
                        @class.Lowerings.Remove(key);
                        return Result<Objects.LoweredClass>.Error(failure);
                    }

                    lowered.Definition.DeclaringType = enclosing!.Definition;
                    enclosing.Definition.NestedTypes.Add(lowered.Definition);
                }

                if (Declare(@class, lowered, target) is { } error)
                {
                    @class.Lowerings.Remove(key);
                    return Result<Objects.LoweredClass>.Error(error);
                }

                Later(key, () => Lower(@class, lowered, target));

                return Result<Objects.LoweredClass>.Ok(lowered);
            });
        }

        /// <summary>
        /// A definition's accessibility, as CLR LIR marks it.
        /// </summary>
        private static LIR.CLR.Accessible Lowered(Package.DotNet.Accessibility accessibility)
            => new(accessibility switch
            {
                Package.DotNet.Accessibility.Public => LIR.CLR.Accessibility.Public,
                Package.DotNet.Accessibility.Private => LIR.CLR.Accessibility.Private,
                Package.DotNet.Accessibility.Internal => LIR.CLR.Accessibility.Internal,
                Package.DotNet.Accessibility.Protected => LIR.CLR.Accessibility.Protected,
                Package.DotNet.Accessibility.ProtectedInternal => LIR.CLR.Accessibility.ProtectedInternal,
                Package.DotNet.Accessibility.PrivateProtected => LIR.CLR.Accessibility.PrivateProtected,
                _ => throw new ArgumentOutOfRangeException(nameof(accessibility)),
            });

        /// <summary>
        /// What the CLR names a method: an accessor for its property, as <c>get_Name</c> and
        /// <c>set_Name</c>, which is how the CLR and the languages on it find a property's
        /// accessors, and how a class implements an interface's property; anything else by
        /// its own name.
        /// </summary>
        private static string ClrName(Package.DotNet.Class owner, Package.DotNet.Method method)
            => owner.Properties.FirstOrDefault(property => ReferenceEquals(property.Getter, method)) is { } read ? "get_" + read.Name
            : owner.Properties.FirstOrDefault(property => ReferenceEquals(property.Setter, method)) is { } written ? "set_" + written.Name
            : method.Name;

        /// <summary>
        /// Declares the members of a class, so that all the code can refer to them.
        /// </summary>
        private Error? Declare(Objects.DefinedClass @class, Objects.LoweredClass lowered, object? target)
        {
            var (node, definition) = (@class.Node, lowered.Definition);

            if (node.Base is not null)
                if (CompileType(node.Base, target).When(out var @base).Error(out var error))
                    return error;
                else
                    definition.Base = @base;

            definition.IsInterface = viewed.ContainsKey(node);

            foreach (var @interface in node.Interfaces)
                if (CompileType(@interface, target).When(out var implemented).Error(out var error))
                    return error;
                else
                    definition.Interfaces.Add(implemented!);

            foreach (var field in node.Fields)
                if (CompileType(field.Type, target).When(out var type).Error(out var error))
                    return error;
                else
                {
                    var declared = new LIR.CLR.ClassField(field.Name, type!) { Metadata = { Lowered(field.Accessibility) } };

                    definition.Fields.Add(declared);
                    lowered.Fields[field] = declared;
                }

            foreach (var constructor in @class.Constructors)
            {
                if (ExplicitThis(constructor.CustomMetadata) && constructor.Parameters.Count == 0)
                    return new ErrorMessage("An explicit this needs a parameter to be it.");

                var code = new LIR.Signature();

                code.Parameters.Add(new LIR.Parameter(definition) { Name = "this" });

                if (Signature(CallParameters(constructor.Parameters, constructor.CustomMetadata), code, target) is { } error)
                    return error;

                var creation = new LIR.Signature { ReturnType = definition };
                creation.Parameters.AddRange(code.Parameters.Skip(1));

                var declared = new LIR.CLR.ClassConstructor(creation, new(code) { Body = new() }) { Metadata = { Lowered(constructor.Accessibility) } };

                definition.Constructors.Add(declared);
                lowered.Constructors.Add(declared);
                signatures[(constructor, target ?? NoTarget)] = [.. code.Parameters];
            }

            foreach (var method in node.Methods)
            {
                var signature = new LIR.Signature();
                var methodParameters = new List<LIR.CLR.ClassGenericParameter>();

                foreach (var parameter in method.GenericParameters)
                {
                    var declaredParameter = new LIR.CLR.ClassGenericParameter(parameter.Name);

                    methodParameters.Add(declaredParameter);
                    lowered.Parameters[parameter] = declaredParameter;
                }

                if (method.IsStatic && ExplicitThis(method.CustomMetadata))
                    return new ErrorMessage($"Method {method.Name} is static, so it has no this.");

                if (ExplicitThis(method.CustomMetadata) && method.Parameters.Count == 0)
                    return new ErrorMessage("An explicit this needs a parameter to be it.");

                if (!method.IsStatic)
                    signature.Parameters.Add(new LIR.Parameter(definition) { Name = "this" });

                if (Signature(CallParameters(method.Parameters, method.CustomMetadata), signature, target) is { } error)
                    return error;

                if (method.ReturnType is not null)
                    if (CompileType(method.ReturnType, target).When(out var type).Error(out error))
                        return error;
                    else
                        signature.ReturnType = type;

                var name = ClrName(node, method);
                var declared = new LIR.CLR.ClassMethod(
                    name,
                    method.IsStatic,
                    new(signature) { Name = name, Body = new() }
                )
                {
                    Metadata = { Lowered(method.Accessibility) },
                    IsVirtual = method.IsVirtual,
                    IsAbstract = method.IsAbstract || definition.IsInterface,
                    IsOverride = method.IsOverride
                };

                declared.GenericParameters.AddRange(methodParameters);

                definition.Methods.Add(declared);
                lowered.Methods[method] = declared;
                signatures[(method, target ?? NoTarget)] = [.. signature.Parameters];
            }

            // A property is the methods of its accessors: a getter that takes nothing and
            // gives its value, and a setter that takes its value and gives nothing.
            foreach (var property in node.Properties)
                if (Property(node, property) is { } invalid)
                    return invalid;
                else if (CompileType(property.Type!, target).When(out var type).Error(out var error))
                    return error;
                else
                    definition.Properties.Add(new(property.Name, type!, property.IsStatic)
                    {
                        Getter = property.Getter is { } getter ? lowered.Methods[getter] : null,
                        Setter = property.Setter is { } setter ? lowered.Methods[setter] : null,
                    });

            // Its nested classes are lowered with it, as the CLR defines them with it.
            foreach (var nested in node.NestedDefinitions.OfType<Package.DotNet.Class>())
                if (Defined(nested).When(out var loaded).Error(out var failure) || Lower((Objects.DefinedClass)loaded!, target).When(out _).Error(out failure))
                    return failure;

            return null;
        }

        /// <summary>
        /// The types of the parameters a call passes arguments to.
        /// </summary>
        private Error? Signature(List<HIR.Code.Parameter> parameters, LIR.Signature signature, object? target)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Type is null)
                    return new ErrorMessage($"Parameter {parameter.Name} needs a type.");

                if (CompileType(parameter.Type, target).When(out var type).Error(out var error))
                    return error;

                signature.Parameters.Add(new LIR.Parameter(type!) { Name = parameter.Name });
            }

            return null;
        }

        /// <summary>
        /// Lowers the code of a class: its constructors, which give its fields their values
        /// first, and its methods.
        /// </summary>
        private Error? Lower(Objects.DefinedClass @class, Objects.LoweredClass lowered, object? target)
        {
            var node = @class.Node;

            foreach (var (constructor, declared) in @class.Constructors.Zip(lowered.Constructors))
            {
                using var bound = Bind(@class, constructor.This, constructor.Parameters, constructor.CustomMetadata, signatures[(constructor, target ?? NoTarget)], target);

                foreach (var field in node.Fields)
                    if (field.DefaultValue is not null && Initialize(field, @class, constructor, declared, target) is { } error)
                        return error;

                if (constructor.Body is { } body && Lower(body, declared.Function, null, constructor.Parameters, target) is { } invalid)
                    return invalid;
            }

            foreach (var (method, declared) in lowered.Methods)
            {
                if (declared.IsAbstract)
                    continue;

                if (method.Body is null)
                    return new ErrorMessage($"Method {method.Name} has no body.");

                using var bound = Bind(@class, method.IsStatic ? null : method.This, method.Parameters, method.CustomMetadata, signatures[(method, target ?? NoTarget)], target);

                if (Lower(method.Body, declared.Function, method.ReturnType, method.Parameters, target) is { } error)
                    return error;
            }

            return null;
        }

        /// <summary>
        /// Makes a member's parameters the variables its code uses, as the target lowered
        /// them: its object first, which an explicit this parameter is too. They are its
        /// until the binding is disposed, which gives back what they were before, as a
        /// member lowered for one target can be lowered for another while it is.
        /// </summary>
        private Binding Bind(
            Objects.DefinedClass? @class,
            Package.DotNet.This? @this,
            List<HIR.Code.Parameter> parameters,
            List<Package.DotNet.CustomMetadata> metadata,
            List<LIR.Parameter> lowered,
            object? target
        )
        {
            var called = CallParameters(parameters, metadata);
            var index = 0;
            var binding = new Binding();

            if (@this is not null && @class is not null)
            {
                var receiver = new Objects.Variable(Lowered(@this, lowered[0], target), () => @class);

                binding.Set(@this, receiver);

                if (ExplicitThis(metadata) && parameters.Count > 0)
                    binding.Set(parameters[0], receiver);

                index = 1;
            }

            foreach (var parameter in called)
            {
                var variable = new Objects.Variable(
                    Lowered(parameter, lowered[index++], target),
                    () => Scope.HIRLoader.LoadType(parameter.Type!).Unwrap()
                );

                // A parameter that takes a reference is, to its code, a variable of what
                // the reference refers to.
                binding.Set(parameter, parameter.Type is Package.DotNet.RefType reference
                    && Scope.HIRLoader.LoadType(reference).Ok(out var loaded) && loaded is Objects.RefType referenced
                        ? new Objects.Referenced(this, variable, referenced.Element)
                        : variable);
            }

            return binding;
        }

        /// <summary>
        /// What each target lowered a parameter to, by its node, as its variable finds it.
        /// </summary>
        private readonly Dictionary<HIR.Instance, Dictionary<object, LIR.Variable>> parameterLowerings = new(ReferenceEqualityComparer.Instance);

        private static readonly object NoParameterTarget = new();

        private Func<object?, LIR.Variable> Lowered(HIR.Instance node, LIR.Variable variable, object? target)
        {
            if (!parameterLowerings.TryGetValue(node, out var lowerings))
                parameterLowerings[node] = lowerings = [];

            lowerings[target ?? NoParameterTarget] = variable;

            return at => lowerings.TryGetValue(at ?? NoParameterTarget, out var found) ? found : variable;
        }

        /// <summary>
        /// Variables set for a while, which disposing it sets back to what they were.
        /// </summary>
        private sealed class Binding()
            : IDisposable
        {
            private readonly List<(HIR.Instance Node, CompilerObject? Was)> set = [];

            public void Set(HIR.Instance node, CompilerObject value)
                => set.Add((node, CodeLoaders.Define(node, value)));

            public void Dispose()
            {
                for (var index = set.Count - 1; index >= 0; index--)
                    CodeLoaders.Redefine(set[index].Node, set[index].Was);
            }
        }

        private Error? Initialize(
            Package.DotNet.Field field,
            Objects.DefinedClass @class,
            Package.DotNet.Constructor constructor,
            LIR.CLR.ClassConstructor declared,
            object? target
        )
        {
            if (Scope.HIRCodeLoader.LoadExpression(field.DefaultValue!).When(out var value).Error(out var error))
                return error;

            if (Scope.HIRLoader.LoadInstance(constructor.This).When(out var @this).Error(out error))
                return error;

            if (@class.Member(@this!, field.Name).When(out var member).Error(out error))
                return error;

            if (Scope.CG.Set(member!, value!).When(out var set).Error(out error))
                return error;

            if (Scope.IR.CompileStatement(set!, target).When(out var statement).Error(out error))
                return error;

            declared.Function.Body!.Block.Statements.Add(statement!);

            return null;
        }

        private IResult<LIR.Type, Error> CompileType(HIR.Type type, object? target)
            => Scope.HIRLoader.LoadType(type).When(out var loaded).Error(out var error)
                ? Result<LIR.Type>.Error(error)
                : Scope.IR.CompileType(loaded!, target);

        // ---- Modules

        private readonly Dictionary<Package.DotNet.Module, Objects.DefinedModule> definedModules = [];

        /// <summary>
        /// A module defined in Z#: an LIR module of each target's, whose functions and
        /// globals are its own, and whose initializer gives its globals their values. Its
        /// classes are classes defined in Z#, each lowered as it is reached.
        /// </summary>
        private IResult DefineModule(Package.DotNet.Module node)
        {
            if (definedModules.TryGetValue(node, out var known))
                return Result.Ok(known);

            if (node.Properties.Count > 0)
                return Result.Error($"Module {node.Name} has properties, which can't be defined yet.");

            if (node.Definitions.FirstOrDefault(definition => definition is not (Package.DotNet.Class or Package.DotNet.Interface)) is { } other)
                return Result.Error($"Module {node.Name} has a {other.GetType().Name}, which can't be defined yet.");

            return Result.Ok(definedModules[node] = new Objects.DefinedModule(this, node));
        }

        /// <summary>
        /// A function of a module defined in Z#, if the method is one.
        /// </summary>
        internal CompilerObject? ModuleFunction(Package.DotNet.Method node)
            => definedModules.Values.FirstOrDefault(module => module.Node.Methods.Contains(node)) is { } module
                ? new Objects.ModuleFunction(this, node, module)
                : null;

        /// <summary>
        /// A <c>let</c> of a module defined in Z#, if the field is one.
        /// </summary>
        internal CompilerObject? ModuleGlobal(Package.DotNet.Field node)
            => definedModules.Values.FirstOrDefault(module => module.Node.Fields.Contains(node)) is { } module
                ? new Objects.ModuleGlobal(module, node)
                : null;

        internal IResult<Objects.LoweredModule, Error> Lower(Objects.DefinedModule module, object? target)
        {
            if (module.Lowerings.TryGetValue(target ?? NoTarget, out var known))
                return Result<Objects.LoweredModule>.Ok(known);

            if (target is not LIR.CLR.ClrPlatform)
                return Result<Objects.LoweredModule>.Error($"Module {module.Node.Name} can only be compiled for a CLR platform.");

            return Lowering(target, key =>
            {
                var node = module.Node;
                var lowered = new Objects.LoweredModule(new LIR.Module(node.Name.Length > 0 ? node.Name : null));

                module.Lowerings[key] = lowered;

                if (Declare(module, lowered, target) is { } error)
                {
                    module.Lowerings.Remove(key);
                    return Result<Objects.LoweredModule>.Error(error);
                }

                Later(key, () => Lower(module, lowered, target));

                return Result<Objects.LoweredModule>.Ok(lowered);
            });
        }

        private Error? Declare(Objects.DefinedModule module, Objects.LoweredModule lowered, object? target)
        {
            foreach (var field in module.Node.Fields)
            {
                if (CompileType(field.Type, target).When(out var type).Error(out var error))
                    return error;

                var global = new LIR.Global(type!) { Name = field.Name, Metadata = { Lowered(field.Accessibility) } };

                lowered.Module.Globals.Add(global);
                lowered.Globals[field] = global;
            }

            foreach (var method in module.Node.Methods)
            {
                var signature = new LIR.Signature();

                if (Signature(method.Parameters, signature, target) is { } error)
                    return error;

                if (method.ReturnType is not null)
                    if (CompileType(method.ReturnType, target).When(out var type).Error(out error))
                        return error;
                    else
                        signature.ReturnType = type;

                var function = new LIR.Function(signature) { Name = method.Name, Body = new(), Metadata = { Lowered(method.Accessibility) } };

                lowered.Module.Functions.Add(function);
                lowered.Functions[method] = function;
            }

            // A module lowers its classes too, which are its members, so each is checked and
            // lowered with it.
            foreach (var definition in module.Node.Definitions)
                if (Defined(definition as Package.DotNet.Class ?? View((Package.DotNet.Interface)definition)).When(out var loaded).Error(out var error))
                    return error;
                else if (Lower((Objects.DefinedClass)loaded!, target).When(out _).Error(out error))
                    return error;

            return null;
        }

        private Error? Lower(Objects.DefinedModule module, Objects.LoweredModule lowered, object? target)
        {
            foreach (var method in module.Node.Methods)
            {
                if (method.Body is null)
                    return new ErrorMessage($"Function {method.Name} has no body.");

                var function = lowered.Functions[method];

                using var bound = Bind(null, null, method.Parameters, [], [.. function.Signature.Parameters], target);

                if (Lower(method.Body, function, method.ReturnType, method.Parameters, target) is { } error)
                    return error;
            }

            // The module's initializer gives its globals their values, which its platform
            // runs before any of them is used.
            foreach (var field in module.Node.Fields)
            {
                if (field.DefaultValue is null)
                    continue;

                if (Scope.HIRCodeLoader.LoadExpression(field.DefaultValue).When(out var value).Error(out var error))
                    return error;

                if (Scope.CG.Set(new Objects.ModuleGlobal(module, field), value!).When(out var set).Error(out error))
                    return error;

                if (Scope.IR.CompileStatement(set!, target).When(out var statement).Error(out error))
                    return error;

                lowered.Module.Initializer.Block.Statements.Add(statement!);
            }

            return null;
        }
    }
}
