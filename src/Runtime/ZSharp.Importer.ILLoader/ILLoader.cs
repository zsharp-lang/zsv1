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
        /// <summary>
        /// What describes the CLR entities code uses, as the CLR handles every CLR platform
        /// realizes.
        /// </summary>
        private readonly LIR.CLR.Reflected reflected = new();

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


        private readonly Dictionary<MethodInfo, Package.DotNet.Method> methods = [];
        private readonly Dictionary<Package.DotNet.Method, MethodInfo> methodInfos = [];

        private readonly Dictionary<ConstructorInfo, Package.DotNet.Constructor> constructors = [];
        private readonly Dictionary<Package.DotNet.Constructor, ConstructorInfo> constructorInfos = [];
        private readonly Dictionary<Type, LIR.CLR.ClrCode> boxes = [];
        private readonly Dictionary<LIR.Type, LIR.CLR.ClrCode> typeObjects = [];
        private readonly Dictionary<(FieldInfo Field, bool IsSet), LIR.CLR.ClrCode> fieldAccessors = [];

        public void Install(MappedLoader loader, MappedCodeLoader code)
        {
            running = this;

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

            // A module is an instance, loaded to one CO, which LoadModule keeps.
            loader.Instances.Register<Package.DotNet.Module>(LoadModule);

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

        /// <summary>
        /// The description of a type: of one the runtime made of a definition defined in Z#,
        /// the definition, so a type has one description however it is reached.
        /// </summary>
        public Package.DotNet.TypeReference Load(Type type)
        {
            if (!typeNodes.TryGetValue(type, out var result))
            {
                if (DefinedAs(type) is { } defined)
                    return typeNodes[type] = defined;

                typeNodes[type] = result = Describe(type);
                nodeTypes[result] = type;
            }

            return result;
        }

        /// <summary>
        /// The loader of the process that runs, which code describing a type asks.
        /// </summary>
        private static ILLoader? running;

        /// <summary>
        /// The description of a type, as code asks for it as it runs, wherever it runs: the
        /// loader of that process describes the type there. A description of a type is a
        /// call of this, given the type's type object.
        /// </summary>
        public static Package.DotNet.TypeReference DescribeType(Type type)
            => (running ?? throw new InvalidOperationException("No loader describes types in this process.")).Load(type);

        private static readonly MethodInfo describeType = typeof(ILLoader).GetMethod(nameof(DescribeType))!;

        /// <summary>
        /// The description of a method, as code asks for it as it runs, as a type's is.
        /// </summary>
        public static Package.DotNet.Method DescribeMethod(MethodBase method)
            => (running ?? throw new InvalidOperationException("No loader describes methods in this process.")).Load((MethodInfo)method);

        private static readonly MethodInfo describeMethod = typeof(ILLoader).GetMethod(nameof(DescribeMethod))!;

        /// <summary>
        /// The description of a method, as code: a call of <see cref="DescribeMethod"/>,
        /// given the method object its token gives, cast to the type asked for if it is
        /// narrower.
        /// </summary>
        internal IResult DescribedMethod(MethodInfo method, CompilerObject targetType)
        {
            var description = Class(typeof(Package.DotNet.Method));
            var token = new LIR.CLR.ClrCode(new LIR.Signature { ReturnType = Handle(typeof(MethodBase)) })
                .Emit(System.Reflection.Emit.OpCodes.Ldtoken, Handle(method))
                .Emit(System.Reflection.Emit.OpCodes.Call, Handle(typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), [typeof(RuntimeMethodHandle)])!));
            var methodObject = new Objects.Call(token, [], () => Class(typeof(MethodBase)));
            var call = new Objects.Call(Handle(describeMethod), [methodObject], () => description);

            return Core.Compiler.ModuleScope.TS.IsAssignableTo(targetType, description)
                ? Result.Ok(call)
                : Core.Compiler.ModuleScope.CG.Cast(call, targetType);
        }

        /// <summary>
        /// The description of a type, as code: a call of <see cref="DescribeType"/>, given the
        /// type's type object, cast to the type asked for if it is narrower.
        /// </summary>
        internal IResult Described(CompilerObject type, CompilerObject targetType)
        {
            var description = Class(typeof(Package.DotNet.TypeReference));
            var call = new Objects.Call(Handle(describeType), [type], () => description);

            return Core.Compiler.ModuleScope.TS.IsAssignableTo(targetType, description)
                ? Result.Ok(call)
                : Core.Compiler.ModuleScope.CG.Cast(call, targetType);
        }

        /// <summary>
        /// The CLR entity a node describes, if it describes one: a type, a method, a
        /// constructor or a module the loader described, or <see langword="null"/>, as for
        /// a definition, which describes what it defines.
        /// </summary>
        public object? Described(HIR.Instance node)
            => node switch
            {
                Package.DotNet.TypeReference type when nodeTypes.TryGetValue(type, out var found) => found,
                Package.DotNet.Method method when methodInfos.TryGetValue(method, out var found) => found,
                Package.DotNet.Constructor constructor when constructorInfos.TryGetValue(constructor, out var found) => found,
                Package.DotNet.Module module when moduleInfos.TryGetValue(module, out var found) => found,
                _ => null,
            };

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

        private readonly Dictionary<(string, Type, Type), LIR.CLR.ClrCode> instructions = [];

        /// <summary>
        /// A function of one value whose code is instructions, as a conversion is, made once
        /// for each operation and pair of types.
        /// </summary>
        private LIR.CLR.ClrCode Instruction(string operation, Type from, Type to, Action<LIR.CLR.ClrCode> emit)
        {
            if (instructions.TryGetValue((operation, from, to), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(to) };
            signature.Parameters.Add(new(Handle(from)) { Name = "value" });

            var code = new LIR.CLR.ClrCode(signature);
            emit(code);

            return instructions[(operation, from, to)] = code;
        }

        /// <summary>
        /// The function that converts a number from one numeric type to another.
        /// </summary>
        internal LIR.CLR.ClrCode Convert(Type from, Type to)
            => Instruction("convert", from, to, code => Conversions.Emit(code, from, to));

        private readonly Dictionary<(Type, LIR.Type), LIR.CLR.ClrCode> downcasts = [];

        /// <summary>
        /// The function that casts a value down to a type it might be of, which fails if it
        /// isn't: a reference is checked, and a value type is unboxed, as <c>unbox.any</c>
        /// does both. The type may be one a platform is still building.
        /// </summary>
        internal LIR.CLR.ClrCode Downcast(Type from, LIR.Type to)
        {
            if (downcasts.TryGetValue((from, to), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = to };
            signature.Parameters.Add(new(Handle(from)) { Name = "value" });

            return downcasts[(from, to)] = new LIR.CLR.ClrCode(signature).Emit(System.Reflection.Emit.OpCodes.Unbox_Any, to);
        }

        private readonly Dictionary<(Type, LIR.Type), LIR.CLR.ClrCode> typeTests = [];

        /// <summary>
        /// The function that tells whether a value is of a type. <c>null</c> is of none.
        /// The type may be one a platform is still building.
        /// </summary>
        internal LIR.CLR.ClrCode IsOf(Type from, LIR.Type to)
        {
            if (typeTests.TryGetValue((from, to), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(typeof(bool)) };
            signature.Parameters.Add(new(Handle(from)) { Name = "value" });

            return typeTests[(from, to)] = new LIR.CLR.ClrCode(signature)
                .Emit(System.Reflection.Emit.OpCodes.Isinst, to)
                .Emit(System.Reflection.Emit.OpCodes.Ldnull)
                .Emit(System.Reflection.Emit.OpCodes.Cgt_Un);
        }

        private readonly Dictionary<(Type, object), LIR.CLR.ClrCode> delegates = [];

        /// <summary>
        /// The function that makes a delegate of a type for a method of a class a platform
        /// is still building, given the object it is bound to.
        /// </summary>
        internal LIR.CLR.ClrCode Delegate(Type delegateType, LIR.CLR.ClassMethod method)
            => Delegate(delegateType, method, !method.IsStatic && (method.IsVirtual || method.IsAbstract || method.IsOverride));

        /// <summary>
        /// The function that makes a delegate of a type for a method, given the object it
        /// is bound to.
        /// </summary>
        internal LIR.CLR.ClrCode Delegate(Type delegateType, MethodInfo method)
            => Delegate(delegateType, Handle(method), method.IsVirtual && !method.IsStatic);

        /// <summary>
        /// Its argument is the object the method is bound to, or null for a static method. A
        /// virtual method is found through the object, so the delegate calls what the object
        /// overrides.
        /// </summary>
        private LIR.CLR.ClrCode Delegate(Type delegateType, object method, bool isVirtual)
        {
            if (delegates.TryGetValue((delegateType, method), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(delegateType) };
            signature.Parameters.Add(new(Handle(typeof(object))) { Name = "target" });

            var code = new LIR.CLR.ClrCode(signature);

            if (isVirtual)
                code.Emit(System.Reflection.Emit.OpCodes.Dup).Emit(System.Reflection.Emit.OpCodes.Ldvirtftn, method);
            else
                code.Emit(System.Reflection.Emit.OpCodes.Ldftn, method);

            code.Emit(System.Reflection.Emit.OpCodes.Newobj, Handle(delegateType.GetConstructor([typeof(object), typeof(nint)])!));

            return delegates[(delegateType, method)] = code;
        }

        /// <summary>
        /// A parameter of code being loaded, as the variable that code uses. A kind of
        /// parameter defined elsewhere registers this as its instance loader.
        /// </summary>
        public IResult LoadParameter(HIR.Code.Parameter parameter)
            => LoadVariable(parameter);


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
                ("|", "op_BitwiseOr"), ("&", "op_BitwiseAnd"),
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

        internal LIR.Type Handle(Type type)
            => reflected.Type(type);

        internal LIR.CLR.ClrMethod Handle(MethodBase method)
            => reflected.Method(method);

        private readonly Dictionary<FieldInfo, LIR.CLR.ClrCode> fieldAddresses = [];

        /// <summary>
        /// The function that gives the address of a field, of the object it is given if the
        /// field isn't static.
        /// </summary>
        internal LIR.CLR.ClrCode FieldAddress(FieldInfo field)
        {
            if (fieldAddresses.TryGetValue(field, out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(field.FieldType.MakeByRefType()) };

            if (!field.IsStatic)
                signature.Parameters.Add(new(Handle(field.DeclaringType!)) { Name = "this" });

            return fieldAddresses[field] = new LIR.CLR.ClrCode(signature)
                .Emit(field.IsStatic ? System.Reflection.Emit.OpCodes.Ldsflda : System.Reflection.Emit.OpCodes.Ldflda, reflected.Field(field));
        }

        private readonly Dictionary<(string, Type), LIR.CLR.ClrCode> comparisons = [];

        /// <summary>
        /// The function that compares two values of a type by the instruction C# compares
        /// them with: enums by their values, signed or not, and references by identity.
        /// </summary>
        internal LIR.CLR.ClrCode Comparison(string name, Type operand)
        {
            if (comparisons.TryGetValue((name, operand), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(typeof(bool)) };
            signature.Parameters.Add(new(Handle(operand)) { Name = "left" });
            signature.Parameters.Add(new(Handle(operand)) { Name = "right" });

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

            var code = new LIR.CLR.ClrCode(signature).Emit(instruction);

            if (negate)
                code.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0).Emit(System.Reflection.Emit.OpCodes.Ceq);

            return comparisons[(name, operand)] = code;
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
        internal LIR.CLR.ClrCode LoadIndirect(Type element)
            => Instruction("ldobj", element.MakeByRefType(), element, code => code.Emit(System.Reflection.Emit.OpCodes.Ldobj, Handle(element)));

        private readonly Dictionary<Type, LIR.CLR.ClrCode> stores = [];

        internal LIR.CLR.ClrCode StoreIndirect(Type element)
        {
            if (stores.TryGetValue(element, out var known))
                return known;

            var signature = new LIR.Signature();
            signature.Parameters.Add(new(Handle(element.MakeByRefType())) { Name = "reference" });
            signature.Parameters.Add(new(Handle(element)) { Name = "value" });

            return stores[element] = new LIR.CLR.ClrCode(signature).Emit(System.Reflection.Emit.OpCodes.Stobj, Handle(element));
        }

        private readonly Dictionary<(Type, int), LIR.CLR.ClrCode> arrays = [];

        /// <summary>
        /// The function that makes an array of its arguments, all of one type. They are on
        /// the stack, so it keeps them in locals, the last first, to store them in order.
        /// </summary>
        internal LIR.CLR.ClrCode ArrayOf(Type element, int count)
        {
            if (arrays.TryGetValue((element, count), out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(element.MakeArrayType()) };

            for (var index = 0; index < count; index++)
                signature.Parameters.Add(new(Handle(element)) { Name = $"element{index}" });

            var code = new LIR.CLR.ClrCode(signature);
            var type = Handle(element);

            for (var index = 0; index < count; index++)
                code.Locals.Add(type);

            for (var index = count - 1; index >= 0; index--)
                code.Emit(System.Reflection.Emit.OpCodes.Stloc, new LIR.CLR.ClrLocal(index));

            code.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, count).Emit(System.Reflection.Emit.OpCodes.Newarr, type);

            for (var index = 0; index < count; index++)
                code.Emit(System.Reflection.Emit.OpCodes.Dup)
                    .Emit(System.Reflection.Emit.OpCodes.Ldc_I4, index)
                    .Emit(System.Reflection.Emit.OpCodes.Ldloc, new LIR.CLR.ClrLocal(index))
                    .Emit(System.Reflection.Emit.OpCodes.Stelem, type);

            return arrays[(element, count)] = code;
        }

        internal LIR.CLR.ClrCode Handle(FieldInfo field, bool isSet)
        {
            if (fieldAccessors.TryGetValue((field, isSet), out var known))
                return known;

            var signature = new LIR.Signature();

            if (!field.IsStatic)
                signature.Parameters.Add(new(Handle(field.DeclaringType!)) { Name = "this" });

            if (isSet)
                signature.Parameters.Add(new(Handle(field.FieldType)) { Name = "value" });
            else
                signature.ReturnType = Handle(field.FieldType);

            var opcode = (isSet, field.IsStatic) switch
            {
                (false, false) => System.Reflection.Emit.OpCodes.Ldfld,
                (false, true) => System.Reflection.Emit.OpCodes.Ldsfld,
                (true, false) => System.Reflection.Emit.OpCodes.Stfld,
                (true, true) => System.Reflection.Emit.OpCodes.Stsfld,
            };

            return fieldAccessors[(field, isSet)] = new LIR.CLR.ClrCode(signature).Emit(opcode, reflected.Field(field));
        }

        /// <summary>
        /// A function that turns a value of a value type into an object.
        /// </summary>
        internal LIR.CLR.ClrCode Box(Type type)
        {
            if (boxes.TryGetValue(type, out var known))
                return known;

            var signature = new LIR.Signature { ReturnType = Handle(typeof(object)) };

            signature.Parameters.Add(new(Handle(type)) { Name = "value" });

            return boxes[type] = new LIR.CLR.ClrCode(signature).Emit(System.Reflection.Emit.OpCodes.Box, Handle(type));
        }

        /// <summary>
        /// A function that gives the type object of a type: the value of a class. It works
        /// for a type still being defined too.
        /// </summary>
        internal LIR.CLR.ClrCode TypeOf(LIR.Type type)
        {
            if (typeObjects.TryGetValue(type, out var known))
                return known;

            return typeObjects[type] = new LIR.CLR.ClrCode(new LIR.Signature { ReturnType = Handle(typeof(Type)) })
                .Emit(System.Reflection.Emit.OpCodes.Ldtoken, type)
                .Emit(System.Reflection.Emit.OpCodes.Call, Handle(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!));
        }

        internal static Type? ReturnType(MethodBase method)
            => method switch
            {
                ConstructorInfo constructor => constructor.DeclaringType,
                MethodInfo { ReturnType: var type } when type != typeof(void) => type,
                _ => null
            };

        private IResult LoadModule(Package.DotNet.Module node)
            => moduleInfos.TryGetValue(node, out var module)
                ? Result.Ok(new Objects.Module(this, module))
                : DefineModule(node);

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

        private Error? Property(Package.DotNet.Class owner, Package.DotNet.Property property)
        {
            if (property is { Getter: null, Setter: null } || property.Type is null)
                return new ErrorMessage($"Property {property.Name} of {owner.Name} needs a getter with a return type, or a setter that takes its value.");

            if (property.Getter is { } getter && (getter.ReturnType is null || CallParameters(getter.Parameters, getter.CustomMetadata).Count > 0))
                return new ErrorMessage($"The getter of property {property.Name} of {owner.Name} takes nothing and gives its value.");

            if (property.Setter is { } setter && (setter.ReturnType is not null || CallParameters(setter.Parameters, setter.CustomMetadata).Count != 1))
                return new ErrorMessage($"The setter of property {property.Name} of {owner.Name} takes one value and returns nothing.");

            if (property is { Getter: { } read, Setter: { } write } && read.IsStatic != write.IsStatic)
                return new ErrorMessage($"The accessors of property {property.Name} of {owner.Name} are both static or both not.");

            return null;
        }

        /// <summary>
        /// Whether a type is one that describes types, as an HIR type is: where a type goes
        /// as what describes it, as in a signature, rather than as its type object.
        /// </summary>
        /// <summary>
        /// A type as an instance of a generic definition has it: a type parameter is what
        /// it is given, and a type made of them is made of what they are given.
        /// </summary>
        internal CompilerObject Substitute(Type type, Func<Type, CompilerObject?> given)
        {
            if (type.IsGenericParameter && given(type) is { } argument)
                return argument;

            if (!type.ContainsGenericParameters)
                return Class(type);

            if (type.IsGenericType)
            {
                var arguments = type.GetGenericArguments().Select(argument => Substitute(argument, given)).ToArray();

                return arguments.All(argument => argument.Is<IILType>(out _))
                    ? Class(type.GetGenericTypeDefinition().MakeGenericType([.. arguments.Select(argument => argument.As<IILType>()!.GetILType())]))
                    : new Objects.Instantiation(this, type.GetGenericTypeDefinition(), arguments);
            }

            return Class(typeof(object));
        }

        internal bool Describes(CompilerObject type)
            => Core.Compiler.ModuleScope.TS.IsAssignableTo(Class(typeof(ZSharp.HIR.Type)), type);

        /// <summary>
        /// The description of a generic type's instance, as code that builds it from what
        /// describes each argument, or <see langword="null"/> if an argument has no
        /// description.
        /// </summary>
        internal IResult? DescribeInstance(Type definition, IEnumerable<CompilerObject> arguments)
        {
            var description = Class(typeof(ZSharp.HIR.Type));
            var described = new List<Argument>();

            foreach (var argument in arguments)
                if (Core.Compiler.ModuleScope.CG.ImplicitCast(argument, description).When(out var converted).Error(out _))
                    return null;
                else
                    described.Add(new Argument(converted!));

            return Core.Compiler.ModuleScope.CG.Call(
                new Objects.Method(this, typeof(Package.DotNet.GenericInstance).GetMethod(nameof(Package.DotNet.GenericInstance.Of))!, null),
                [new Argument(new Objects.Literal(Load(definition), typeof(Package.DotNet.TypeReference), this)), .. described]
            );
        }

        /// <summary>
        /// A generic type given its arguments, made from the types its parts stand for.
        /// </summary>
        private IResult Instantiate(Package.DotNet.GenericInstance node)
        {
            if (LoadType(node.Definition).When(out var definition).Error(out var error))
                return Result.Error(error);

            var arguments = new List<CompilerObject>();

            foreach (var argument in node.Arguments)
                if (Core.Compiler.ModuleScope.HIRLoader.LoadType(argument).When(out var loaded).Error(out error))
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
                return Defined(View(@interface));

            if (node is Package.DotNet.GenericParameter given && Substituted(given) is { } argument)
                return Result.Ok(argument);

            if (node is Package.DotNet.GenericParameter parameter)
                return typeParameters.TryGetValue(parameter, out var known)
                    ? Result.Ok(known)
                    : Result.Error($"Type parameter {parameter.Name} can only be used inside its definition.");

            return node is Package.DotNet.Class definition
                ? Defined(definition)
                : Result.Error(new UnsupportedError($"{node.GetType().Name} can't be defined yet."));
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
        /// Lowers code into a function on the CLR: it checks that the code returns what the
        /// function returns, and a return converts its value to that. The function's
        /// parameters are the lowerer's, so its code reaches them as the lowerer loads them.
        /// </summary>
        private Error? Lower(HIR.Code.Code body, LIR.Function function, HIR.Type? returned, List<HIR.Code.Parameter> parameters, object? target)
        {
            if (Returns.Check(body.Block, returned is not null) is { } invalid)
                return invalid;

            if (Cells(parameters, function, target) is { } failed)
                return failed;

            var outer = CodeLoaders.Returns;

            CodeLoaders.Returns = null;

            if (returned is not null)
                if (Core.Compiler.ModuleScope.HIRLoader.LoadType(returned).When(out var type).Error(out var failure))
                    return failure;
                else
                    CodeLoaders.Returns = type;

            try
            {
                if (Core.Compiler.ModuleScope.HIRCodeLoader.LoadStatement(body.Block).When(out var loaded).Error(out var error))
                    return error;

                if (Core.Compiler.ModuleScope.IR.CompileStatement(loaded!, target).When(out var statement).Error(out error))
                    return error;

                function.Body!.Block.Statements.Add(statement!);

                return null;
            }
            finally
            {
                CodeLoaders.Returns = outer;
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

            // A local function's captures are the running process's cells, so it runs there.
            var capturing = signature.Parameters.Count;

            if (Signature(node.Parameters, signature, runtime) is { } invalid)
                return Result<LIR.Function>.Error(invalid);

            Bind(null, null, node.Parameters, [], [.. signature.Parameters.Skip(capturing)], runtime);

            if (node.ReturnType is not null)
                if (CompileType(node.ReturnType, runtime).When(out var type).Error(out var error))
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
                if (Lower(node.Body, function, node.ReturnType, node.Parameters, runtime) is { } error)
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
        private Error? Cells(List<HIR.Code.Parameter> parameters, LIR.Function function, object? target)
        {
            foreach (var parameter in parameters.Where(parameter => parameter.IsCaptured))
            {
                if (!variables.TryGetValue(parameter, out var known) || known is not Objects.Variable variable)
                    continue;

                var type = ((ITyped)variable).Type;

                if (Core.Compiler.ModuleScope.IR.CompileType(type, target).When(out var compiled).Error(out var error))
                    return error;

                var cell = new LIR.Local(new LIR.Reference(compiled!)) { Name = parameter.Name };

                function.Body!.Block.Locals.Add(cell);
                function.Body.Block.Statements.Add(new LIR.SetLocal(cell, new LIR.Allocate(new LIR.GetLocal(variable.For(target)))));
                variables[parameter] = new Objects.Cell(cell, type);
            }

            return null;
        }

        private Error? Check(Package.DotNet.Class node)
        {
            if (node.StaticConstructor is not null)
                return new ErrorMessage($"Class {node.Name} has a static constructor, which can't be defined yet.");

            if (node.NestedDefinitions.Count > 0)
                return new ErrorMessage($"Class {node.Name} has members that can't be defined yet.");

            if (node.Base is Package.DotNet.TypeReference known && nodeTypes.TryGetValue(known, out var type) && type.IsSealed)
                return new ErrorMessage($"Class {node.Name} can't derive from {type.Name}, which is sealed.");

            var seen = new HashSet<Package.DotNet.ClassReference>();

            for (var @base = node.Base; @base is Package.DotNet.Class current; @base = current.Base)
                if (!seen.Add(current))
                    return new ErrorMessage($"Class {node.Name} derives from itself.");

            return null;
        }


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
