using System.Reflection;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Describes CLR entities as HIR. HIR holds no reflection and is never lazy, so
    /// the link back to the CLR lives here: a node is mapped to pre-created LIR
    /// handles, and the loaders registered by <see cref="Install"/> return COs
    /// primed with them. Members are discovered by those COs on demand.
    /// </summary>
    public sealed class ILLoader(Platform.Runtime.Runtime runtime)
    {
        private readonly Dictionary<Type, Package.DotNet.TypeReference> typeNodes = [];
        private readonly Dictionary<Package.DotNet.TypeReference, Type> nodeTypes = [];
        private readonly Dictionary<Type, LIR.TypeHandle> types = [];
        private readonly Dictionary<Package.DotNet.Class, Objects.ClassInProgress> inProgress = [];
        private readonly Dictionary<HIR.Instance, Objects.Variable> variables = [];

        private readonly Dictionary<Module, Package.DotNet.Module> modules = [];
        private readonly Dictionary<Package.DotNet.Module, Module> moduleInfos = [];

        private readonly Dictionary<MethodInfo, Package.DotNet.Method> methods = [];
        private readonly Dictionary<Package.DotNet.Method, MethodInfo> methodInfos = [];
        private readonly Dictionary<ConstructorInfo, Package.DotNet.Constructor> constructors = [];
        private readonly Dictionary<Package.DotNet.Constructor, ConstructorInfo> constructorInfos = [];
        private readonly Dictionary<MethodBase, LIR.FunctionHandle> functions = [];
        private readonly Dictionary<(FieldInfo Field, bool IsSet), LIR.FunctionHandle> fieldAccessors = [];

        public void Install(MappedLoader loader, MappedCodeLoader code)
        {
            var ts = Core.Compiler.ModuleScope.TS;
            var isAssignableTo = ts.IsAssignableTo;

            ts.IsAssignableTo = (target, source) =>
                target.Is<Objects.Class>(out var to) && source.Is<Objects.Class>(out var from)
                    ? to.IL.IsAssignableFrom(from.IL)
                    : isAssignableTo(target, source);

            loader.Instances.Register<Package.DotNet.Method>(LoadMethod);
            loader.Definitions.Register<Package.DotNet.Method>(LoadMethod);

            loader.Instances.Register<Package.DotNet.Constructor>(LoadConstructor);
            loader.Definitions.Register<Package.DotNet.Constructor>(LoadConstructor);

            RegisterType<Package.DotNet.Class>(loader);
            RegisterType<Package.DotNet.Struct>(loader);
            RegisterType<Package.DotNet.Interface>(loader);
            RegisterType<Package.DotNet.ArrayType>(loader);
            RegisterType<Package.DotNet.RefType>(loader);

            loader.Definitions.Register<Package.DotNet.Class>(LoadType);
            loader.Definitions.Register<Package.DotNet.Struct>(LoadType);
            loader.Definitions.Register<Package.DotNet.Interface>(LoadType);

            loader.Instances.Register<Package.DotNet.Module>(LoadModule);
            loader.Definitions.Register<Package.DotNet.Module>(LoadModule);

            loader.Instances.Register<Package.DotNet.ExposedObject>(LoadExposed);
            loader.Instances.Register<Package.DotNet.Parameter>(LoadVariable);
            loader.Instances.Register<Package.DotNet.This>(LoadVariable);

            code.Expressions.Register<HIR.Code.StringLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, typeof(string), this))
            );
            code.Expressions.Register<HIR.Code.BooleanLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, typeof(bool), this))
            );
        }

        public Package.DotNet.Module Load(Module module)
        {
            if (!modules.TryGetValue(module, out var result))
            {
                modules[module] = result = new(module.Name);
                moduleInfos[result] = module;
            }

            return result;
        }

        public Package.DotNet.TypeReference Load(Type type)
        {
            if (!typeNodes.TryGetValue(type, out var result))
            {
                typeNodes[type] = result = Describe(type);
                nodeTypes[result] = type;
            }

            return result;
        }

        public Package.DotNet.Method Load(MethodInfo method)
        {
            if (!methods.TryGetValue(method, out var result))
            {
                methods[method] = result = new(method.Name) { IsStatic = method.IsStatic };
                methodInfos[result] = method;
            }

            return result;
        }

        public Package.DotNet.Constructor Load(ConstructorInfo constructor)
        {
            if (!constructors.TryGetValue(constructor, out var result))
            {
                constructors[constructor] = result = new();
                constructorInfos[result] = constructor;
            }

            return result;
        }

        public Package.DotNet.ExposedObject Expose(object value)
            => new(value, Load(value.GetType()));

        internal Objects.Class Class(Type type)
            => new(this, type);

        internal LIR.TypeHandle Handle(Type type)
        {
            if (!types.TryGetValue(type, out var handle))
            {
                types[type] = handle = new();
                runtime.Map(handle, type);
            }

            return handle;
        }

        internal LIR.FunctionHandle Handle(MethodBase method)
        {
            if (functions.TryGetValue(method, out var handle))
                return handle;

            var signature = new LIR.Signature();

            if (!method.IsStatic && method is MethodInfo)
                signature.Parameters.Add(new(Handle(method.DeclaringType!)) { Name = "this" });

            foreach (var parameter in method.GetParameters())
                signature.Parameters.Add(new(Handle(parameter.ParameterType)) { Name = parameter.Name });

            signature.ReturnType = ReturnType(method) is { } type ? Handle(type) : null;

            functions[method] = handle = new(signature);
            runtime.Map(handle, method);

            return handle;
        }

        internal LIR.FunctionHandle Handle(FieldInfo field, bool isSet)
        {
            if (fieldAccessors.TryGetValue((field, isSet), out var handle))
                return handle;

            var signature = new LIR.Signature();

            if (!field.IsStatic)
                signature.Parameters.Add(new(Handle(field.DeclaringType!)) { Name = "this" });

            if (isSet)
                signature.Parameters.Add(new(Handle(field.FieldType)) { Name = "value" });
            else
                signature.ReturnType = Handle(field.FieldType);

            fieldAccessors[(field, isSet)] = handle = new(signature);

            var opcode = (isSet, field.IsStatic) switch
            {
                (false, false) => System.Reflection.Emit.OpCodes.Ldfld,
                (false, true) => System.Reflection.Emit.OpCodes.Ldsfld,
                (true, false) => System.Reflection.Emit.OpCodes.Stfld,
                (true, true) => System.Reflection.Emit.OpCodes.Stsfld,
            };

            runtime.Map(handle, il => il.Emit(opcode, field));

            return handle;
        }

        internal static Type? ReturnType(MethodBase method)
            => method switch
            {
                ConstructorInfo constructor => constructor.DeclaringType,
                MethodInfo { ReturnType: var type } when type != typeof(void) => type,
                _ => null
            };

        private IResult LoadModule(Package.DotNet.Module node)
        {
            if (moduleInfos.TryGetValue(node, out var module))
                return Result.Ok(new Objects.Module(this, module));

            if (node.Fields.Count > 0 || node.Properties.Count > 0 || node.Methods.Count > 0)
                return Result.Error($"Module {node.Name} has members that can't be defined yet.");

            if (node.Definitions.FirstOrDefault(definition => definition is not Package.DotNet.Class) is { } other)
                return Result.Error($"Module {node.Name} has a {other.GetType().Name}, which can't be defined yet.");

            return Define(node.Definitions.Cast<Package.DotNet.Class>()) is { } error
                ? Result.Error(error)
                : Result.Ok(new Objects.DefinedModule(this, node));
        }

        /// <summary>
        /// Gets a class that was defined, as a type that now exists.
        /// </summary>
        internal Objects.Class Class(Package.DotNet.Class node)
            => Class(nodeTypes[node]);

        private Package.DotNet.TypeReference Describe(Type type)
        {
            var name = type.FullName ?? type.Name;

            return type switch
            {
                { IsArray: true } => new Package.DotNet.ArrayType(Load(type.GetElementType()!)),
                { IsByRef: true } => new Package.DotNet.RefType(Load(type.GetElementType()!)),
                { IsInterface: true } => new Package.DotNet.Interface(name),
                { IsValueType: true } => new Package.DotNet.Struct(name),
                _ => new Package.DotNet.Class(name),
            };
        }

        private void RegisterType<T>(MappedLoader loader)
            where T : class, Package.DotNet.TypeReference
        {
            loader.Instances.Register<T>(LoadType);
            loader.Types.Register<T>(LoadType);
        }

        private IResult LoadType(Package.DotNet.TypeReference node)
        {
            if (nodeTypes.TryGetValue(node, out var type))
                return Result.Ok(Class(type));

            return node is Package.DotNet.Class definition
                ? Define(definition)
                : Result.Error(new UnsupportedError($"{node.GetType().Name} can't be defined yet."));
        }

        /// <summary>
        /// Loads a class being defined into the runtime. It is then an existing type,
        /// so from here on it is reached like any other.
        /// </summary>
        private IResult Define(Package.DotNet.Class node)
        {
            if (inProgress.TryGetValue(node, out var loading))
                return Result.Ok(loading);

            return Define([node]) is { } error
                ? Result.Error(error)
                : Result.Ok(Class(nodeTypes[node]));
        }

        /// <summary>
        /// Loads classes together with those they reach, as they can refer to each other.
        /// </summary>
        private Error? Define(IEnumerable<Package.DotNet.Class> roots)
        {
            var nodes = new List<Package.DotNet.Class>();

            foreach (var root in roots)
                Reach(root, nodes);

            foreach (var node in nodes)
                if (Check(node) is { } error)
                    return error;

            foreach (var node in nodes)
                inProgress[node] = new(node, new(node.Name));

            try
            {
                foreach (var node in nodes)
                    if (Declare(inProgress[node]) is { } error)
                        return error;

                foreach (var node in nodes)
                    if (Lower(inProgress[node]) is { } error)
                        return error;

                var loaded = runtime.Load([.. nodes.Select(node => inProgress[node].Definition)]);

                foreach (var (node, type) in nodes.Zip(loaded))
                {
                    nodeTypes[node] = type;
                    typeNodes[type] = node;
                }

                return null;
            }
            finally
            {
                foreach (var node in nodes)
                    inProgress.Remove(node);
            }
        }

        /// <summary>
        /// Declares the members of a class, so that all the code can refer to them.
        /// </summary>
        private Error? Declare(Objects.ClassInProgress @class)
        {
            var (node, definition) = (@class.Node, @class.Definition);

            if (node.Base is not null)
                if (CompileType(node.Base).When(out var @base).Error(out var error))
                    return error;
                else
                    definition.Base = @base;

            foreach (var field in node.Fields)
                if (CompileType(field.Type).When(out var type).Error(out var error))
                    return error;
                else
                    definition.Fields.Add(new(field.Name, type!));

            foreach (var constructor in @class.Constructors)
            {
                var creation = new LIR.Signature { ReturnType = definition };
                var code = new LIR.Signature();

                var @this = new LIR.Parameter(definition) { Name = "this" };
                code.Parameters.Add(@this);
                variables[constructor.This] = new(@this, () => @class);

                if (Parameters(constructor.Parameters, code) is { } error)
                    return error;

                creation.Parameters.AddRange(code.Parameters.Skip(1));

                definition.Constructors.Add(new(creation, new(code) { Body = new() }));
            }

            foreach (var method in node.Methods)
            {
                var signature = new LIR.Signature();

                if (method.This is { } self)
                {
                    var @this = new LIR.Parameter(definition) { Name = "this" };
                    signature.Parameters.Add(@this);
                    variables[self] = new(@this, () => @class);
                }

                if (Parameters(method.Parameters, signature) is { } error)
                    return error;

                if (method.ReturnType is not null)
                    if (CompileType(method.ReturnType).When(out var type).Error(out error))
                        return error;
                    else
                        signature.ReturnType = type;

                var declared = new Platform.Runtime.ClassMethod(
                    method.Name,
                    method.IsStatic,
                    new(signature) { Name = method.Name, Body = new() }
                );

                definition.Methods.Add(declared);
                @class.Methods[method] = declared;
            }

            return null;
        }

        private Error? Parameters(List<Package.DotNet.Parameter> parameters, LIR.Signature signature)
        {
            foreach (var parameter in parameters)
            {
                if (CompileType(parameter.Type).When(out var type).Error(out var error))
                    return error;

                var compiled = new LIR.Parameter(type!) { Name = parameter.Name };

                signature.Parameters.Add(compiled);
                variables[parameter] = new(
                    compiled,
                    () => Core.Compiler.ModuleScope.HIRLoader.LoadType(parameter.Type).Unwrap()
                );
            }

            return null;
        }

        /// <summary>
        /// Lowers the code of a class. Everything it can reach has been declared by now.
        /// </summary>
        private Error? Lower(Objects.ClassInProgress @class)
        {
            var (node, definition) = (@class.Node, @class.Definition);

            foreach (var (constructor, declared) in @class.Constructors.Zip(definition.Constructors))
            {
                foreach (var field in node.Fields)
                    if (field.DefaultValue is not null && Initialize(field, @class, constructor, declared) is { } error)
                        return error;

                if (constructor.Body is { } body && Lower(body, declared.Function) is { } invalid)
                    return invalid;
            }

            foreach (var (method, declared) in @class.Methods)
                if (method.Body is null)
                    return new ErrorMessage($"Method {method.Name} has no body.");
                else if (Lower(method.Body, declared.Function) is { } error)
                    return error;

            return null;
        }

        private Error? Initialize(
            Package.DotNet.Field field,
            Objects.ClassInProgress @class,
            Package.DotNet.Constructor constructor,
            Platform.Runtime.ClassConstructor declared
        )
        {
            if (Core.Compiler.ModuleScope.HIRCodeLoader.LoadExpression(field.DefaultValue!).When(out var value).Error(out var error))
                return error;

            if (Core.Compiler.ModuleScope.HIRLoader.LoadInstance(constructor.This).When(out var @this).Error(out error))
                return error;

            if (@class.Member(@this!, field.Name).When(out var target).Error(out error))
                return error;

            if (Core.Compiler.ModuleScope.CG.Set(target!, value!).When(out var set).Error(out error))
                return error;

            if (Core.Compiler.ModuleScope.IR.CompileStatement(set!, runtime).When(out var statement).Error(out error))
                return error;

            declared.Function.Body!.Block.Statements.Add(statement!);

            return null;
        }

        private Error? Lower(HIR.Code.Code body, LIR.Function function)
        {
            if (Core.Compiler.ModuleScope.HIRCodeLoader.LoadStatement(body.Block).When(out var loaded).Error(out var error))
                return error;

            if (Core.Compiler.ModuleScope.IR.CompileStatement(loaded!, runtime).When(out var statement).Error(out error))
                return error;

            function.Body!.Block.Statements.Add(statement!);

            return null;
        }

        private void Reach(Package.DotNet.Class node, List<Package.DotNet.Class> nodes)
        {
            if (nodeTypes.ContainsKey(node) || nodes.Contains(node))
                return;

            nodes.Add(node);

            if (node.Base is Package.DotNet.Class @base)
                Reach(@base, nodes);

            foreach (var field in node.Fields)
                if (field.Type is Package.DotNet.Class type)
                    Reach(type, nodes);
        }

        private Error? Check(Package.DotNet.Class node)
        {
            if (node.StaticConstructor is not null)
                return new ErrorMessage($"Class {node.Name} has a static constructor, which can't be defined yet.");

            if (node.Interfaces.Count > 0 || node.Properties.Count > 0 || node.NestedDefinitions.Count > 0)
                return new ErrorMessage($"Class {node.Name} has members that can't be defined yet.");

            if (node.Base is Package.DotNet.TypeReference known && nodeTypes.TryGetValue(known, out var type) && type.IsSealed)
                return new ErrorMessage($"Class {node.Name} can't derive from {type.Name}, which is sealed.");

            var seen = new HashSet<Package.DotNet.ClassReference>();

            for (var @base = node.Base; @base is Package.DotNet.Class current; @base = current.Base)
                if (!seen.Add(current))
                    return new ErrorMessage($"Class {node.Name} derives from itself.");

            return null;
        }

        private IResult<LIR.Type, Error> CompileType(HIR.Type type)
            => Core.Compiler.ModuleScope.HIRLoader.LoadType(type).When(out var loaded).Error(out var error)
                ? Result<LIR.Type>.Error(error)
                : Core.Compiler.ModuleScope.IR.CompileType(loaded!, runtime);

        private IResult LoadMethod(Package.DotNet.Method node)
            => methodInfos.TryGetValue(node, out var method)
                ? Result.Ok(new Objects.MethodGroup(this, [method], null))
                : Result.Error($"Method {node.Name} can only be reached through its class.");

        private IResult LoadConstructor(Package.DotNet.Constructor node)
            => constructorInfos.TryGetValue(node, out var constructor)
                ? Result.Ok(new Objects.MethodGroup(this, [constructor], null))
                : Result.Error("A constructor can only be reached through its class.");

        private IResult LoadVariable(HIR.Instance node)
            => variables.TryGetValue(node, out var variable)
                ? Result.Ok(variable)
                : Result.Error("A parameter can only be used by the code it belongs to.");

        private IResult LoadExposed(Package.DotNet.ExposedObject node)
            => Result.Ok(new Objects.Literal(node.Value, nodeTypes[node.Type], this));
    }
}
