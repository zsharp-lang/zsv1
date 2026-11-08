using System.Reflection;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Describes CLR entities as HIR. HIR holds no reflection and is never lazy, so
    /// the link back to the CLR lives here: a node is mapped to pre-created LIR
    /// handles, and the loaders registered by <see cref="Install"/> return COs
    /// primed with them. Members are discovered by those COs on demand.
    /// </summary>
    public sealed partial class ILLoader(Platform.Runtime.Runtime runtime)
    {
        private readonly Dictionary<Type, Package.DotNet.TypeReference> typeNodes = [];
        private readonly Dictionary<Package.DotNet.TypeReference, Type> nodeTypes = [];
        private readonly Dictionary<Type, LIR.TypeHandle> types = [];
        private readonly Dictionary<Package.DotNet.Class, Objects.ClassInProgress> inProgress = [];

        /// <summary>
        /// An interface is defined through a class-shaped view of it, which the class
        /// machinery handles; its type is then the interface's.
        /// </summary>
        private readonly Dictionary<Package.DotNet.Interface, Package.DotNet.Class> views = [];

        /// <summary>
        /// The type parameters of the generic classes defined, as their code uses them.
        /// </summary>
        private readonly Dictionary<Package.DotNet.GenericParameter, Objects.GenericParameterType> typeParameters = [];
        private readonly Dictionary<Package.DotNet.Class, Package.DotNet.Interface> viewed = [];
        private readonly Dictionary<HIR.Instance, CompilerObject> variables = [];

        /// <summary>
        /// The local functions lowered so far.
        /// </summary>
        private readonly Dictionary<Package.DotNet.LocalFunction, LIR.Function> localFunctions = [];

        /// <summary>
        /// What each capture is inside the local function being lowered, innermost first.
        /// </summary>
        private readonly Stack<Dictionary<HIR.Code.Capture, CompilerObject>> captures = [];

        private readonly Dictionary<Module, Package.DotNet.Module> modules = [];
        private readonly Dictionary<Package.DotNet.Module, Module> moduleInfos = [];

        /// <summary>
        /// What the code being lowered returns, if it returns anything.
        /// </summary>
        private CompilerObject? returnType;

        private Loader<HIR.Code.Return>? loadReturn;

        private readonly Dictionary<MethodInfo, Package.DotNet.Method> methods = [];
        private readonly Dictionary<Package.DotNet.Method, MethodInfo> methodInfos = [];

        /// <summary>
        /// The parameters of each method and constructor defined from HIR, as written, with
        /// default values that are code.
        /// </summary>
        private readonly Dictionary<MethodBase, List<HIR.Code.Parameter>> definedParameters = [];
        private readonly Dictionary<ConstructorInfo, Package.DotNet.Constructor> constructors = [];
        private readonly Dictionary<Package.DotNet.Constructor, ConstructorInfo> constructorInfos = [];
        private readonly Dictionary<MethodBase, LIR.FunctionHandle> functions = [];
        private readonly Dictionary<Type, LIR.FunctionHandle> boxes = [];
        private readonly Dictionary<LIR.Type, LIR.FunctionHandle> typeObjects = [];
        private readonly Dictionary<(FieldInfo Field, bool IsSet), LIR.FunctionHandle> fieldAccessors = [];

        public void Install(MappedLoader loader, MappedCodeLoader code)
        {
            loader.Instances.Register<Package.DotNet.Method>(LoadMethod);
            loader.Definitions.Register<Package.DotNet.Method>(LoadMethod);

            loader.Instances.Register<Package.DotNet.Constructor>(LoadConstructor);
            loader.Definitions.Register<Package.DotNet.Constructor>(LoadConstructor);

            RegisterType<Package.DotNet.Class>(loader);
            RegisterType<Package.DotNet.Struct>(loader);
            RegisterType<Package.DotNet.Interface>(loader);
            RegisterType<Package.DotNet.ArrayType>(loader);
            RegisterType<Package.DotNet.RefType>(loader);
            RegisterType<Package.DotNet.GenericInstance>(loader);
            RegisterType<Package.DotNet.GenericParameter>(loader);

            loader.Definitions.Register<Package.DotNet.Class>(LoadType);
            loader.Definitions.Register<Package.DotNet.Struct>(LoadType);
            loader.Definitions.Register<Package.DotNet.Interface>(LoadType);

            loader.Instances.Register<Package.DotNet.Module>(LoadModule);
            loader.Definitions.Register<Package.DotNet.Module>(LoadModule);

            loader.Instances.Register<Package.DotNet.ExposedObject>(LoadExposed);
            loader.Instances.Register<Package.DotNet.Operator>(node => Result.Ok(new Objects.ClrOperator(this, node.Name)));
            loader.Instances.Register<Package.DotNet.LiftedOperator>(node => Result.Ok(new Objects.LiftedOperator(this, node.Operator, node.Lifting)));
            loader.Instances.Register<Package.DotNet.RefFamily>(node => Result.Ok(new Objects.RefFamily(this, node.IsOut)));
            loader.Instances.Register<Package.DotNet.Field>(field => ModuleGlobal(field) is { } global
                ? Result.Ok(global)
                : Result.Error($"Field {field.Name} can only be reached through its class."));
            loader.Instances.Register<Package.DotNet.Parameter>(LoadVariable);
            loader.Instances.Register<HIR.Code.PositionalParameter>(LoadVariable);
            loader.Instances.Register<HIR.Code.NamedParameter>(LoadVariable);

            // A parameter as defined is what a call matches arguments to, and as an instance
            // it is the variable the code it belongs to uses.
            loader.Definitions.Register<Package.DotNet.Parameter>(node => Result.Ok(new Objects.DefinedParameter(node)));

            loader.Definitions.Register<Package.DotNet.LocalFunction>(node => Result.Ok(new Objects.LocalFunction(this, node)));
            loader.Instances.Register<Package.DotNet.LocalFunction>(node => Core.Compiler.ModuleScope.HIRLoader.LoadDefinition(node));

            // Inside a function that captures it, a capture is the cell it was given.
            var outside = loader.Instances.Register<HIR.Code.Capture>(capture =>
                captures.TryPeek(out var inside) && inside.TryGetValue(capture, out var cell)
                    ? Result.Ok(cell)
                    : Result.Error(new UnsupportedError("A capture is only known to the functions that capture it."))
            );

            if (outside is not null)
                loader.Instances.Register<HIR.Code.Capture>(capture =>
                    captures.TryPeek(out var inside) && inside.TryGetValue(capture, out var cell)
                        ? Result.Ok(cell)
                        : outside(capture)
                );
            loader.Instances.Register<Package.DotNet.This>(LoadVariable);

            code.Expressions.Register<HIR.Code.StringLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, typeof(string), this))
            );
            code.Expressions.Register<HIR.Code.BooleanLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, typeof(bool), this))
            );
            code.Expressions.Register<HIR.Code.IntegerLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, typeof(int), this))
            );
            code.Expressions.Register<HIR.Code.DecimalLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, typeof(double), this))
            );
            code.Expressions.Register<HIR.Code.NullLiteral>(_ => Result.Ok(new Objects.Null(this)));
            code.Expressions.Register<HIR.Code.ArrayLiteral>(LoadArray);

            loadReturn = code.Statements.Register<HIR.Code.Return>(LoadReturn);
        }

        /// <summary>
        /// A return converts its value to what the code being lowered returns.
        /// </summary>
        private IResult LoadReturn(HIR.Code.Return statement)
        {
            if (statement.Value is null || returnType is null)
                return loadReturn!(statement);

            if (Core.Compiler.ModuleScope.HIRCodeLoader.LoadExpression(statement.Value).When(out var value).Error(out var error))
                return Result.Error(error);

            return Core.Compiler.ModuleScope.CG.ImplicitCast(value!, returnType).Ok(out var converted)
                ? Result.Ok(new Objects.Return(converted))
                : Result.Error("A return value doesn't match what is returned.");
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

        internal Platform.Runtime.Runtime Runtime => runtime;

        /// <summary>
        /// The extension methods of the classes imports have reached. A host adds it to the
        /// dispatcher of external members.
        /// </summary>
        public ExtensionMethods Extensions => extensions ??= new(this);

        private ExtensionMethods? extensions;

        private readonly Dictionary<(string, Type, Type), LIR.FunctionHandle> instructions = [];

        /// <summary>
        /// A function of one value that the runtime maps to instructions, as a conversion is,
        /// made once for each operation and pair of types.
        /// </summary>
        private LIR.FunctionHandle Instruction(string operation, Type from, Type to, Action<System.Reflection.Emit.ILGenerator> emit)
        {
            if (instructions.TryGetValue((operation, from, to), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(to) };
            signature.Parameters.Add(new(Handle(from)) { Name = "value" });

            var handle = new LIR.FunctionHandle(signature);
            runtime.Map(handle, emit);

            return instructions[(operation, from, to)] = handle;
        }

        /// <summary>
        /// The function that converts a number from one numeric type to another.
        /// </summary>
        internal LIR.FunctionHandle Convert(Type from, Type to)
            => Instruction("convert", from, to, il => Conversions.Emit(il, from, to));

        /// <summary>
        /// The function that casts a value to a type it might be of, which fails if it is
        /// not: a reference is checked, and a value type is unboxed.
        /// </summary>
        internal LIR.FunctionHandle Downcast(Type from, Type to)
            => Instruction("downcast", from, to, il =>
                il.Emit(to.IsValueType ? System.Reflection.Emit.OpCodes.Unbox_Any : System.Reflection.Emit.OpCodes.Castclass, to));

        /// <summary>
        /// The function that tells whether a value is of a type. <c>null</c> is of none.
        /// </summary>
        internal LIR.FunctionHandle IsOf(Type from, Type to)
            => Instruction($"is {to.AssemblyQualifiedName}", from, typeof(bool), il =>
            {
                il.Emit(System.Reflection.Emit.OpCodes.Isinst, to);
                il.Emit(System.Reflection.Emit.OpCodes.Ldnull);
                il.Emit(System.Reflection.Emit.OpCodes.Cgt_Un);
            });

        private readonly Dictionary<(Type, MethodInfo), LIR.FunctionHandle> delegates = [];

        /// <summary>
        /// The function that makes a delegate of a type for a method, given the object it
        /// is bound to.
        /// </summary>
        internal LIR.FunctionHandle Delegate(Type delegateType, MethodInfo method)
        {
            if (delegates.TryGetValue((delegateType, method), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(delegateType) };
            signature.Parameters.Add(new(Handle(typeof(object))) { Name = "target" });

            var handle = new LIR.FunctionHandle(signature);
            runtime.MapDelegate(handle, delegateType, method);

            return delegates[(delegateType, method)] = handle;
        }

        /// <summary>
        /// A parameter of code being loaded, as the variable that code uses. A kind of
        /// parameter defined elsewhere registers this as its instance loader.
        /// </summary>
        public IResult LoadParameter(HIR.Code.Parameter parameter)
            => LoadVariable(parameter);

        /// <summary>
        /// The parameters a method was defined with from HIR, if it was.
        /// </summary>
        internal List<HIR.Code.Parameter>? DefinedParameters(MethodBase method)
            => definedParameters.TryGetValue(method, out var parameters) ? parameters : null;

        /// <summary>
        /// The operators CLR types define, by the syntax each stands for, so an operator in
        /// Z# applies to what its operands' types define, as in C#.
        /// </summary>
        public IEnumerable<(string Operator, HIR.Instance Function)> ClrOperators()
            => new (string, string)[]
            {
                ("+", "op_Addition"), ("-", "op_Subtraction"), ("*", "op_Multiply"), ("/", "op_Division"),
                ("<", "op_LessThan"), (">", "op_GreaterThan"), ("<=", "op_LessThanOrEqual"), (">=", "op_GreaterThanOrEqual"),
                ("==", "op_Equality"), ("!=", "op_Inequality"),
            }.Select(pair => (pair.Item1, (HIR.Instance)new Package.DotNet.Operator(pair.Item2)));

        /// <summary>
        /// The operators that exist, lifted to nullable operands as C# lifts them, each an
        /// overload of the operator it lifts.
        /// </summary>
        public IEnumerable<(string Operator, HIR.Instance Function)> LiftedOperators(IReadOnlyDictionary<string, HIR.Code.OverloadGroup> operators)
            => new (string, Package.DotNet.Lifting)[]
            {
                ("+", Package.DotNet.Lifting.Value), ("-", Package.DotNet.Lifting.Value),
                ("*", Package.DotNet.Lifting.Value), ("/", Package.DotNet.Lifting.Value),
                ("==", Package.DotNet.Lifting.Equality), ("!=", Package.DotNet.Lifting.Equality),
                ("<", Package.DotNet.Lifting.Comparison), (">", Package.DotNet.Lifting.Comparison),
                ("<=", Package.DotNet.Lifting.Comparison), (">=", Package.DotNet.Lifting.Comparison),
            }
            .Where(pair => operators.ContainsKey(pair.Item1))
            .Select(pair => (pair.Item1, (HIR.Instance)new Package.DotNet.LiftedOperator(operators[pair.Item1], pair.Item2)))
            .ToArray();

        /// <summary>
        /// The functions the methods of a type say operators stand for.
        /// </summary>
        public IEnumerable<(string Operator, HIR.Instance Function)> Operators(Type type)
            => type
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .SelectMany(method => method
                    .GetCustomAttributes<OperatorAttribute>()
                    .Select(attribute => (attribute.Operator, (HIR.Instance)Load(method)))
                );

        public Package.DotNet.ExposedObject Expose(object value)
            => new(value, Load(value.GetType()));

        internal Objects.Class Class(Type type)
            => new(this, type);

        /// <summary>
        /// The type of a reference to a variable of a type: <c>Ref[T]</c>, or <c>Out[T]</c>
        /// for a parameter the callee only assigns.
        /// </summary>
        internal Objects.RefType Ref(Type element, bool isOut)
            => new(this, element, isOut);

        /// <summary>
        /// The type a parameter has as it takes arguments: a <c>ref</c> or <c>out</c>
        /// parameter takes a reference.
        /// </summary>
        internal CompilerObject ParameterType(ParameterInfo parameter)
            => parameter.ParameterType.IsByRef
                ? Ref(parameter.ParameterType.GetElementType()!, parameter.IsOut)
                : Class(parameter.ParameterType);

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

        private readonly Dictionary<FieldInfo, LIR.FunctionHandle> fieldAddresses = [];

        /// <summary>
        /// The function that gives the address of a field, of the object it is given if the
        /// field isn't static.
        /// </summary>
        internal LIR.FunctionHandle FieldAddress(FieldInfo field)
        {
            if (fieldAddresses.TryGetValue(field, out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(field.FieldType.MakeByRefType()) };

            if (!field.IsStatic)
                signature.Parameters.Add(new(Handle(field.DeclaringType!)) { Name = "this" });

            var handle = new LIR.FunctionHandle(signature);

            runtime.Map(handle, il => il.Emit(field.IsStatic ? System.Reflection.Emit.OpCodes.Ldsflda : System.Reflection.Emit.OpCodes.Ldflda, field));

            return fieldAddresses[field] = handle;
        }

        private readonly Dictionary<(string, Type), LIR.FunctionHandle> comparisons = [];

        /// <summary>
        /// The function that compares two values of a type by the instruction C# compares
        /// them with: enums by their values, signed or not, and references by identity.
        /// </summary>
        internal LIR.FunctionHandle Comparison(string name, Type operand)
        {
            if (comparisons.TryGetValue((name, operand), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(typeof(bool)) };
            signature.Parameters.Add(new(Handle(operand)) { Name = "left" });
            signature.Parameters.Add(new(Handle(operand)) { Name = "right" });

            var handle = new LIR.FunctionHandle(signature);
            var unsigned = operand.IsEnum && Enum.GetUnderlyingType(operand) is var underlying
                && (underlying == typeof(byte) || underlying == typeof(ushort) || underlying == typeof(uint) || underlying == typeof(ulong));

            var (instruction, negate) = name switch
            {
                "op_Equality" => (System.Reflection.Emit.OpCodes.Ceq, false),
                "op_Inequality" => (System.Reflection.Emit.OpCodes.Ceq, true),
                "op_LessThan" => (unsigned ? System.Reflection.Emit.OpCodes.Clt_Un : System.Reflection.Emit.OpCodes.Clt, false),
                "op_GreaterThan" => (unsigned ? System.Reflection.Emit.OpCodes.Cgt_Un : System.Reflection.Emit.OpCodes.Cgt, false),
                "op_LessThanOrEqual" => (unsigned ? System.Reflection.Emit.OpCodes.Cgt_Un : System.Reflection.Emit.OpCodes.Cgt, true),
                "op_GreaterThanOrEqual" => (unsigned ? System.Reflection.Emit.OpCodes.Clt_Un : System.Reflection.Emit.OpCodes.Clt, true),
                _ => throw new NotSupportedException($"{name} is not a comparison.")
            };

            runtime.Map(handle, il =>
            {
                il.Emit(instruction);

                if (negate)
                {
                    il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
                    il.Emit(System.Reflection.Emit.OpCodes.Ceq);
                }
            });

            return comparisons[(name, operand)] = handle;
        }

        private IResult LoadArray(HIR.Code.ArrayLiteral array)
        {
            var items = new List<CompilerObject>();

            foreach (var item in array.Items)
                if (Core.Compiler.ModuleScope.HIRCodeLoader.LoadExpression(item).When(out var loaded).Error(out var error))
                    return Result.Error(error);
                else
                    items.Add(loaded!);

            return Result.Ok(new Objects.ArrayLiteral(this, [.. items]));
        }

        /// <summary>
        /// The functions that read and write what a reference refers to.
        /// </summary>
        internal LIR.FunctionHandle LoadIndirect(Type element)
            => Instruction("ldobj", element.MakeByRefType(), element, il => il.Emit(System.Reflection.Emit.OpCodes.Ldobj, element));

        private readonly Dictionary<Type, LIR.FunctionHandle> stores = [];

        internal LIR.FunctionHandle StoreIndirect(Type element)
        {
            if (stores.TryGetValue(element, out var known))
                return known;

            var signature = new LIR.Signature();
            signature.Parameters.Add(new(Handle(element.MakeByRefType())) { Name = "reference" });
            signature.Parameters.Add(new(Handle(element)) { Name = "value" });

            var handle = new LIR.FunctionHandle(signature);

            runtime.Map(handle, il => il.Emit(System.Reflection.Emit.OpCodes.Stobj, element));

            return stores[element] = handle;
        }

        private readonly Dictionary<(Type, int), LIR.FunctionHandle> arrays = [];

        /// <summary>
        /// The function that makes an array of its arguments, all of one type.
        /// </summary>
        internal LIR.FunctionHandle ArrayOf(Type element, int count)
        {
            if (arrays.TryGetValue((element, count), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(element.MakeArrayType()) };

            for (var index = 0; index < count; index++)
                signature.Parameters.Add(new(Handle(element)) { Name = $"element{index}" });

            var handle = new LIR.FunctionHandle(signature);

            runtime.Map(handle, il =>
            {
                var elements = Enumerable.Range(0, count).Select(_ => il.DeclareLocal(element)).ToArray();

                for (var index = count - 1; index >= 0; index--)
                    il.Emit(System.Reflection.Emit.OpCodes.Stloc, elements[index]);

                il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, count);
                il.Emit(System.Reflection.Emit.OpCodes.Newarr, element);

                for (var index = 0; index < count; index++)
                {
                    il.Emit(System.Reflection.Emit.OpCodes.Dup);
                    il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, index);
                    il.Emit(System.Reflection.Emit.OpCodes.Ldloc, elements[index]);
                    il.Emit(System.Reflection.Emit.OpCodes.Stelem, element);
                }
            });

            return arrays[(element, count)] = handle;
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

        /// <summary>
        /// A function that turns a value of a value type into an object.
        /// </summary>
        internal LIR.FunctionHandle Box(Type type)
        {
            if (boxes.TryGetValue(type, out var handle))
                return handle;

            var signature = new LIR.Signature { ReturnType = Handle(typeof(object)) };

            signature.Parameters.Add(new(Handle(type)) { Name = "value" });

            boxes[type] = handle = new(signature);

            runtime.Map(handle, il => il.Emit(System.Reflection.Emit.OpCodes.Box, type));

            return handle;
        }

        /// <summary>
        /// A function that gives the type object of a type: the value of a class.
        /// </summary>
        internal LIR.FunctionHandle TypeOf(LIR.Type type)
        {
            if (typeObjects.TryGetValue(type, out var handle))
                return handle;

            typeObjects[type] = handle = new(new LIR.Signature { ReturnType = Handle(typeof(Type)) });

            runtime.MapTypeOf(handle, type);

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

            // A module is defined once: its variables are its own, however often it is reached.
            if (definedModules.TryGetValue(node, out var known))
                return Result.Ok(known);

            if (node.Properties.Count > 0)
                return Result.Error($"Module {node.Name} has properties, which can't be defined yet.");

            if (node.Definitions.FirstOrDefault(definition => definition is not (Package.DotNet.Class or Package.DotNet.Interface)) is { } other)
                return Result.Error($"Module {node.Name} has a {other.GetType().Name}, which can't be defined yet.");

            // The module's own members are declared and lowered along with its classes, so
            // each can use the other.
            var members = new LIR.Module(node.Name.Length > 0 ? node.Name : null);

            if (Define(
                node.Definitions.Select(definition => definition as Package.DotNet.Class ?? View((Package.DotNet.Interface)definition)),
                () => DeclareMembers(node, members),
                () => LowerMembers(node, members)
            ) is { } error)
                return Result.Error(error);

            if (LoadMembers(node, members) is { } invalid)
                return Result.Error(invalid);

            var defined = new Objects.DefinedModule(this, node);
            definedModules[node] = defined;

            return Result.Ok(defined);
        }

        /// <summary>
        /// Gets a class that was defined, as a type that now exists.
        /// </summary>
        internal Objects.Class Class(Package.DotNet.Class node)
            => Class(nodeTypes[node]);

        private Package.DotNet.TypeReference Describe(Type type)
        {
            var name = type.FullName ?? type.Name;

            if (type.IsConstructedGenericType)
            {
                var instance = new Package.DotNet.GenericInstance(Load(type.GetGenericTypeDefinition()));

                instance.Arguments.AddRange(type.GetGenericArguments().Select(Load));

                return instance;
            }

            return type switch
            {
                { IsArray: true } => new Package.DotNet.ArrayType(Load(type.GetElementType()!)),
                { IsByRef: true } => new Package.DotNet.RefType(Load(type.GetElementType()!)),
                { IsInterface: true } => new Package.DotNet.Interface(name),
                { IsValueType: true } => new Package.DotNet.Struct(name),
                _ => new Package.DotNet.Class(name),
            };
        }

        /// <summary>
        /// The class-shaped view an interface is defined through: its methods, and the
        /// interfaces it extends. It has no constructors.
        /// </summary>
        private Package.DotNet.Class View(Package.DotNet.Interface node)
        {
            if (views.TryGetValue(node, out var known))
                return known;

            var view = new Package.DotNet.Class(node.Name);

            view.Methods.AddRange(node.Methods);
            view.Interfaces.AddRange(node.Interfaces);

            views[node] = view;
            viewed[view] = node;

            return view;
        }

        internal bool IsInterface(Package.DotNet.Class node) => viewed.ContainsKey(node);

        /// <summary>
        /// A generic type given its arguments, made from the types its parts stand for.
        /// </summary>
        private IResult Instantiate(Package.DotNet.GenericInstance node)
        {
            if (LoadType(node.Definition).When(out var definition).Error(out var error))
                return Result.Error(error);

            var arguments = new List<CompilerObject>();

            foreach (var argument in node.Arguments)
                if (LoadType(argument).When(out var loaded).Error(out error))
                    return Result.Error(error);
                else
                    arguments.Add(loaded!);

            return Core.Compiler.ModuleScope.CG.Index(definition!, [.. arguments.Select(argument => new Argument(argument))]);
        }

        /// <summary>
        /// The CLR types arguments stand for, if each is a type that exists.
        /// </summary>
        /// <summary>
        /// The CLR types arguments give type parameters, matched as a call's arguments are
        /// to its parameters: each parameter takes the argument with its name, or else the
        /// next positional one. Returns <see langword="null"/> if they don't match, or if an
        /// argument isn't a type.
        /// </summary>
        internal Type[]? Types(Argument[] arguments, Type[] parameters)
        {
            var stream = new ArgumentStream(arguments);
            var types = new List<Type>();

            foreach (var parameter in parameters)
                if ((stream.Pop(parameter.Name) ?? stream.Pop()) is { } argument && argument.Is<IILType>(out var known))
                    types.Add(known.GetILType());
                else
                    return null;

            return stream.HasArguments ? null : [.. types];
        }

        private void RegisterType<T>(MappedLoader loader)
            where T : class, Package.DotNet.TypeReference
        {
            loader.Instances.Register<T>(LoadType);
            loader.Types.Register<T>(LoadType);
        }

        private IResult LoadType(Package.DotNet.TypeReference node)
        {
            // A reference type is not a type of its own, but a reference to its target.
            if (node is Package.DotNet.RefType reference)
                return Core.Compiler.ModuleScope.HIRLoader.LoadType(reference.Target).When(out var target).Error(out var failure)
                    ? Result.Error(failure)
                    : target!.Is<IILType>(out var known) && known.GetILType() is { IsByRef: false } element
                        ? Result.Ok(Ref(element, reference.IsOut))
                        : Result.Error($"A reference can only be to a CLR type.");

            if (nodeTypes.TryGetValue(node, out var type))
                return Result.Ok(Class(type));

            if (node is Package.DotNet.GenericInstance instance)
                return Instantiate(instance);

            if (node is Package.DotNet.Interface @interface)
                return Define(View(@interface));

            if (node is Package.DotNet.GenericParameter parameter)
                return typeParameters.TryGetValue(parameter, out var known)
                    ? Result.Ok(known)
                    : Result.Error($"Type parameter {parameter.Name} can only be used inside its definition.");

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
        /// <param name="declare">Declares more that the classes' code can reach, once they are declared.</param>
        /// <param name="lower">Lowers the code of what was declared, once the classes' code is lowered.</param>
        private Error? Define(IEnumerable<Package.DotNet.Class> roots, Func<Error?>? declare = null, Func<Error?>? lower = null)
        {
            var nodes = new List<Package.DotNet.Class>();

            foreach (var root in roots)
                Reach(root, nodes);

            foreach (var node in nodes)
                if (Check(node) is { } error)
                    return error;

            foreach (var node in nodes)
            {
                var definition = new Platform.Runtime.ClassDefinition(node.Name);

                // A class's type parameters exist before its members are declared, which use them.
                foreach (var parameter in node.GenericParameters)
                {
                    var lowered = new Platform.Runtime.ClassGenericParameter(parameter.Name);

                    definition.GenericParameters.Add(lowered);
                    typeParameters[parameter] = new(parameter, lowered);
                }

                inProgress[node] = new(this, node, definition);
            }

            try
            {
                foreach (var node in nodes)
                    if (Declare(inProgress[node]) is { } error)
                        return error;

                if (declare?.Invoke() is { } undeclared)
                    return undeclared;

                foreach (var node in nodes)
                    if (Lower(inProgress[node]) is { } error)
                        return error;

                if (lower?.Invoke() is { } unlowered)
                    return unlowered;

                var loaded = runtime.Load([.. nodes.Select(node => inProgress[node].Definition)]);

                foreach (var (node, type) in nodes.Zip(loaded))
                {
                    nodeTypes[node] = type;
                    typeNodes[type] = node;

                    if (viewed.TryGetValue(node, out var @interface))
                    {
                        nodeTypes[@interface] = type;
                        typeNodes[type] = @interface;
                    }

                    var @class = inProgress[node];

                    foreach (var (method, declared) in @class.Methods)
                        definedParameters[runtime.Defined(declared)] = CallParameters(method.Parameters, method.CustomMetadata);

                    foreach (var (constructor, declared) in @class.Constructors.Zip(@class.Definition.Constructors))
                        definedParameters[runtime.Defined(declared)] = CallParameters(constructor.Parameters, constructor.CustomMetadata);
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

            definition.IsInterface = viewed.ContainsKey(node);

            foreach (var @interface in node.Interfaces)
                if (CompileType(@interface).When(out var implemented).Error(out var error))
                    return error;
                else
                    definition.Interfaces.Add(implemented!);

            foreach (var field in node.Fields)
                if (CompileType(field.Type).When(out var type).Error(out var error))
                    return error;
                else
                    definition.Fields.Add(new(field.Name, type!));

            foreach (var constructor in @class.Constructors)
            {
                var creation = new LIR.Signature { ReturnType = definition };
                var code = new LIR.Signature();

                if (This(constructor.This, @class, constructor.Parameters, constructor.CustomMetadata, code) is { } error)
                    return error;

                if (Parameters(CallParameters(constructor.Parameters, constructor.CustomMetadata), code) is { } invalid)
                    return invalid;

                creation.Parameters.AddRange(code.Parameters.Skip(1));

                definition.Constructors.Add(new(creation, new(code) { Body = new() }));
            }

            foreach (var method in node.Methods)
            {
                var signature = new LIR.Signature();
                var methodParameters = new List<Platform.Runtime.ClassGenericParameter>();

                foreach (var parameter in method.GenericParameters)
                {
                    var lowered = new Platform.Runtime.ClassGenericParameter(parameter.Name);

                    methodParameters.Add(lowered);
                    typeParameters[parameter] = new(parameter, lowered);
                }

                if (method.IsStatic && ExplicitThis(method.CustomMetadata))
                    return new ErrorMessage($"Method {method.Name} is static, so it has no this.");

                if (!method.IsStatic && This(method.This, @class, method.Parameters, method.CustomMetadata, signature) is { } invalid)
                    return invalid;

                if (Parameters(CallParameters(method.Parameters, method.CustomMetadata), signature) is { } error)
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
                )
                {
                    IsVirtual = method.IsVirtual,
                    IsAbstract = method.IsAbstract || definition.IsInterface,
                    IsOverride = method.IsOverride
                };

                declared.GenericParameters.AddRange(methodParameters);

                definition.Methods.Add(declared);
                @class.Methods[method] = declared;
            }

            return null;
        }

        /// <summary>
        /// Whether metadata makes the first parameter the object the code runs on.
        /// </summary>
        private bool ExplicitThis(List<Package.DotNet.CustomMetadata> metadata)
        {
            var marker = Load(typeof(ExplicitThisAttribute).GetConstructor(Type.EmptyTypes)!);

            return metadata.Any(item => item.Constructor == marker);
        }

        /// <summary>
        /// The parameters a call passes arguments to, which leaves out an explicit this.
        /// </summary>
        internal List<HIR.Code.Parameter> CallParameters(
            List<HIR.Code.Parameter> parameters,
            List<Package.DotNet.CustomMetadata> metadata
        )
            => ExplicitThis(metadata) ? [.. parameters.Skip(1)] : parameters;

        /// <summary>
        /// Maps <c>this</c> to the object the code runs on. An explicit this parameter is
        /// pre-mapped to the same variable rather than given one of its own.
        /// </summary>
        private Error? This(
            Package.DotNet.This @this,
            Objects.ClassInProgress @class,
            List<HIR.Code.Parameter> parameters,
            List<Package.DotNet.CustomMetadata> metadata,
            LIR.Signature signature
        )
        {
            var receiver = new LIR.Parameter(@class.Definition) { Name = "this" };
            var variable = new Objects.Variable(receiver, () => @class);

            signature.Parameters.Add(receiver);
            variables[@this] = variable;

            if (!ExplicitThis(metadata))
                return null;

            if (parameters.Count == 0)
                return new ErrorMessage("An explicit this needs a parameter to be it.");

            variables[parameters[0]] = variable;

            return null;
        }

        private Error? Parameters(List<HIR.Code.Parameter> parameters, LIR.Signature signature)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Type is null)
                    return new ErrorMessage($"Parameter {parameter.Name} needs a type.");

                if (CompileType(parameter.Type).When(out var type).Error(out var error))
                    return error;

                var compiled = new LIR.Parameter(type!) { Name = parameter.Name };
                var variable = new Objects.Variable(
                    compiled,
                    () => Core.Compiler.ModuleScope.HIRLoader.LoadType(parameter.Type!).Unwrap()
                );

                signature.Parameters.Add(compiled);

                // A parameter that takes a reference is, to its code, a variable of what
                // the reference refers to.
                variables[parameter] = parameter.Type is Package.DotNet.RefType reference
                    && Core.Compiler.ModuleScope.HIRLoader.LoadType(reference).Ok(out var loaded) && loaded is Objects.RefType referenced
                        ? new Objects.Referenced(this, variable, referenced.Element)
                        : variable;
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

                if (constructor.Body is { } body && Lower(body, declared.Function, null, constructor.Parameters) is { } invalid)
                    return invalid;
            }

            foreach (var (method, declared) in @class.Methods)
                if (declared.IsAbstract)
                    continue;
                else if (method.Body is null)
                    return new ErrorMessage($"Method {method.Name} has no body.");
                else if (Lower(method.Body, declared.Function, method.ReturnType, method.Parameters) is { } error)
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

        private Error? Lower(HIR.Code.Code body, LIR.Function function, HIR.Type? returned, List<HIR.Code.Parameter> parameters)
        {
            if (Returns.Check(body.Block, returned is not null) is { } invalid)
                return invalid;

            if (Cells(parameters, function) is { } failed)
                return failed;

            var outer = returnType;

            returnType = null;

            if (returned is not null)
                if (Core.Compiler.ModuleScope.HIRLoader.LoadType(returned).When(out var type).Error(out var failure))
                    return failure;
                else
                    returnType = type;

            try
            {
                if (Core.Compiler.ModuleScope.HIRCodeLoader.LoadStatement(body.Block).When(out var loaded).Error(out var error))
                    return error;

                if (Core.Compiler.ModuleScope.IR.CompileStatement(loaded!, runtime).When(out var statement).Error(out error))
                    return error;

                function.Body!.Block.Statements.Add(statement!);

                return null;
            }
            finally
            {
                returnType = outer;
            }
        }

        /// <summary>
        /// Lowers a local function to a function of its own, once. It takes the cell of
        /// each variable it captures, then its own parameters.
        /// </summary>
        internal IResult<LIR.Function, Error> Lower(Package.DotNet.LocalFunction node)
        {
            if (localFunctions.TryGetValue(node, out var known))
                return Result<LIR.Function>.Ok(known);

            if (node.Body is null)
                return Result<LIR.Function>.Error($"Function {node.Name} has no body.");

            var signature = new LIR.Signature();
            var cells = new Dictionary<HIR.Code.Capture, CompilerObject>(ReferenceEqualityComparer.Instance);

            foreach (var capture in node.Captures)
            {
                if (Core.Compiler.ModuleScope.HIRLoader.LoadInstance((HIR.Instance)capture.Captured).When(out var captured).Error(out var error))
                    return Result<LIR.Function>.Error(error);

                if (Core.Compiler.ModuleScope.TS.TypeOf(captured!).When(out var type).Error(out error))
                    return Result<LIR.Function>.Error(error);

                if (Core.Compiler.ModuleScope.IR.CompileType(type!, runtime).When(out var compiled).Error(out error))
                    return Result<LIR.Function>.Error(error);

                var parameter = new LIR.Parameter(new LIR.Reference(compiled!)) { Name = "capture" };

                signature.Parameters.Add(parameter);
                cells[capture] = new Objects.Cell(parameter, type!);
            }

            if (Parameters(node.Parameters, signature) is { } invalid)
                return Result<LIR.Function>.Error(invalid);

            if (node.ReturnType is not null)
                if (CompileType(node.ReturnType).When(out var type).Error(out var error))
                    return Result<LIR.Function>.Error(error);
                else
                    signature.ReturnType = type;

            var function = new LIR.Function(signature) { Name = node.Name, Body = new() };

            new LIR.Module().Functions.Add(function);

            // Known before its body is lowered, so it can call itself.
            localFunctions[node] = function;
            captures.Push(cells);

            try
            {
                if (Lower(node.Body, function, node.ReturnType, node.Parameters) is { } error)
                {
                    localFunctions.Remove(node);
                    return Result<LIR.Function>.Error(error);
                }

                return Result<LIR.Function>.Ok(function);
            }
            finally
            {
                captures.Pop();
            }
        }

        /// <summary>
        /// A parameter that a function inside the code captures is copied into a cell when
        /// the code starts, and the code uses the cell from then on, so it and the
        /// functions that capture it share it.
        /// </summary>
        private Error? Cells(List<HIR.Code.Parameter> parameters, LIR.Function function)
        {
            foreach (var parameter in parameters.Where(parameter => parameter.IsCaptured))
            {
                if (!variables.TryGetValue(parameter, out var known) || known is not Objects.Variable variable)
                    continue;

                var type = ((ITyped)variable).Type;

                if (Core.Compiler.ModuleScope.IR.CompileType(type, runtime).When(out var compiled).Error(out var error))
                    return error;

                var cell = new LIR.Local(new LIR.Reference(compiled!)) { Name = parameter.Name };

                function.Body!.Block.Locals.Add(cell);
                function.Body.Block.Statements.Add(new LIR.SetLocal(cell, new LIR.Allocate(new LIR.GetLocal(variable.Lowered))));
                variables[parameter] = new Objects.Cell(cell, type);
            }

            return null;
        }

        private void Reach(Package.DotNet.Class node, List<Package.DotNet.Class> nodes)
        {
            if (nodeTypes.ContainsKey(node) || nodes.Contains(node))
                return;

            nodes.Add(node);

            if (node.Base is Package.DotNet.Class @base)
                Reach(@base, nodes);

            // An interface that already exists, as one of the CLR's, isn't defined again.
            foreach (var @interface in node.Interfaces.OfType<Package.DotNet.Interface>().Where(@interface => !nodeTypes.ContainsKey(@interface)))
                Reach(View(@interface), nodes);

            foreach (var field in node.Fields)
                if (field.Type is Package.DotNet.Class type)
                    Reach(type, nodes);
        }

        private Error? Check(Package.DotNet.Class node)
        {
            if (node.StaticConstructor is not null)
                return new ErrorMessage($"Class {node.Name} has a static constructor, which can't be defined yet.");

            if (node.Properties.Count > 0 || node.NestedDefinitions.Count > 0)
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
                ? Result.Ok(new Objects.Method(this, method, null))
                : ModuleFunction(node) is { } function
                    ? Result.Ok(function)
                    : Result.Error($"Method {node.Name} can only be reached through its class.");

        private IResult LoadConstructor(Package.DotNet.Constructor node)
            => constructorInfos.TryGetValue(node, out var constructor)
                ? Result.Ok(new Objects.Method(this, constructor, null))
                : Result.Error("A constructor can only be reached through its class.");

        private IResult LoadVariable(HIR.Instance node)
            => variables.TryGetValue(node, out var variable)
                ? Result.Ok(variable)
                : Result.Error("A parameter can only be used by the code it belongs to.");

        private IResult LoadExposed(Package.DotNet.ExposedObject node)
            => Result.Ok(new Objects.Literal(node.Value, nodeTypes[node.Type], this));
    }
}
