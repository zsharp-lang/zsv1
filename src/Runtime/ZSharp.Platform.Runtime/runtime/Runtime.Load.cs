namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<LIR.Module, Emit.TypeBuilder> holders = [];
        private readonly Dictionary<ClassDefinition, Type> classes = [];
        private readonly Dictionary<ClassField, IL.FieldInfo> classFields = [];
        private readonly Dictionary<ClassConstructor, Emit.ConstructorBuilder> classConstructors = [];
        private readonly Dictionary<ClassMethod, Emit.MethodBuilder> classMethods = [];
        private readonly Dictionary<ClassGenericParameter, Type> genericParameters = [];

        /// <summary>
        /// A generic class as its own code refers to it: instantiated over its own type
        /// parameters, as C# emits it.
        /// </summary>
        private readonly Dictionary<ClassDefinition, Type> selves = [];

        /// <summary>
        /// The class each member belongs to, for a member of a generic class, which its
        /// code refers to through the class's self instance.
        /// </summary>
        private readonly Dictionary<object, ClassDefinition> owners = [];
        private readonly HashSet<string> classNames = [];
        private readonly Queue<Action> bodies = [];
        private readonly List<LIR.Structure> pendingStructures = [];
        private readonly List<LIR.Global> pendingGlobals = [];
        private readonly List<LIR.Function> pendingFunctions = [];

        /// <summary>
        /// Loads a module, and whatever it reaches in other modules.
        /// </summary>
        public void Load(LIR.Module module)
        {
            foreach (var structure in module.Structures)
                ImportStructure(structure);

            foreach (var global in module.Globals)
                ImportGlobal(global);

            foreach (var function in module.Functions)
                ImportFunction(function);

            while (bodies.TryDequeue(out var body))
                body();

            Complete();
        }

        /// <summary>
        /// Loads classes that may refer to each other and to themselves.
        /// </summary>
        public Type[] Load(IReadOnlyList<ClassDefinition> definitions)
        {
            var builders = new Dictionary<ClassDefinition, Emit.TypeBuilder>();

            // Everything is declared first, as any of them can refer to any other.
            foreach (var definition in definitions.Where(definition => !classes.ContainsKey(definition)))
                classes[definition] = builders[definition] = module.DefineType(
                    classNames.Add(definition.Name) ? definition.Name : Unique(definition.Name),
                    definition.IsInterface
                        ? IL.TypeAttributes.Public | IL.TypeAttributes.Interface | IL.TypeAttributes.Abstract
                        : IL.TypeAttributes.Public | IL.TypeAttributes.Class
                            | (definition.Methods.Any(method => method.IsAbstract) ? IL.TypeAttributes.Abstract : 0)
                );

            foreach (var (definition, builder) in builders.Where(pair => pair.Key.GenericParameters.Count > 0))
            {
                var parameters = builder.DefineGenericParameters([.. definition.GenericParameters.Select(parameter => parameter.Name)]);

                foreach (var (parameter, type) in definition.GenericParameters.Zip(parameters))
                    genericParameters[parameter] = type;

                selves[definition] = builder.MakeGenericType(parameters);
            }

            foreach (var (definition, builder) in builders)
                DeclareMembers(definition, builder);

            foreach (var definition in builders.Keys)
            {
                var parent = definition.Base is null ? typeof(object) : ImportType(definition.Base);

                foreach (var constructor in definition.Constructors)
                {
                    var il = classConstructors[constructor].GetILGenerator();

                    il.Emit(Emit.OpCodes.Ldarg_0);
                    il.Emit(Emit.OpCodes.Call, BaseConstructor(definition, parent));

                    new CodeEmitter(this, il, constructor.Function).EmitBody();
                }

                foreach (var method in definition.Methods.Where(method => !method.IsAbstract && !definition.IsInterface))
                    new CodeEmitter(this, classMethods[method].GetILGenerator(), method.Function).EmitBody();
            }

            foreach (var definition in builders.Keys)
                CreateClass(definition, builders);

            return [.. definitions.Select(definition => classes[definition])];
        }

        private void DeclareMembers(ClassDefinition definition, Emit.TypeBuilder builder)
        {
            if (!definition.IsInterface)
                builder.SetParent(definition.Base is null ? typeof(object) : ImportType(definition.Base));

            foreach (var @interface in definition.Interfaces)
                builder.AddInterfaceImplementation(ImportType(@interface));

            var implemented = Implemented(definition);

            foreach (var field in definition.Fields)
                classFields[field] = builder.DefineField(field.Name, ImportType(field.Type), IL.FieldAttributes.Public);

            if (definition.GenericParameters.Count > 0)
                foreach (var member in definition.Fields.Cast<object>().Concat(definition.Constructors).Concat(definition.Methods))
                    owners[member] = definition;

            foreach (var constructor in definition.Constructors)
                classConstructors[constructor] = builder.DefineConstructor(
                    IL.MethodAttributes.Public,
                    IL.CallingConventions.Standard,
                    [.. constructor.Signature.Parameters.Select(parameter => ImportType(parameter.Type))]
                );

            foreach (var method in definition.Methods)
            {
                // A generic method is defined before its signature, which uses its type parameters.
                var declared = builder.DefineMethod(method.Name, Attributes(definition, method, implemented));

                if (method.GenericParameters.Count > 0)
                    foreach (var (parameter, type) in method.GenericParameters.Zip(declared.DefineGenericParameters([.. method.GenericParameters.Select(parameter => parameter.Name)])))
                        genericParameters[parameter] = type;

                declared.SetReturnType(ImportType(method.Signature.ReturnType));
                declared.SetParameters([.. method.Signature.Parameters.Skip(method.IsStatic ? 0 : 1).Select(parameter => ImportType(parameter.Type))]);

                classMethods[method] = declared;
            }
        }

        /// <summary>
        /// How a method is declared. A method that implements an interface method by its
        /// name is virtual and final, as C# makes it, so the CLR maps it to the interface's.
        /// </summary>
        private static IL.MethodAttributes Attributes(ClassDefinition definition, ClassMethod method, HashSet<string> implemented)
        {
            var attributes = IL.MethodAttributes.Public | IL.MethodAttributes.HideBySig;

            if (method.IsStatic)
                return attributes | IL.MethodAttributes.Static;

            if (definition.IsInterface || method.IsAbstract)
                return attributes | IL.MethodAttributes.Abstract | IL.MethodAttributes.Virtual | IL.MethodAttributes.NewSlot;

            if (method.IsOverride)
                return attributes | IL.MethodAttributes.Virtual;

            if (method.IsVirtual)
                return attributes | IL.MethodAttributes.Virtual | IL.MethodAttributes.NewSlot;

            return implemented.Contains(method.Name)
                ? attributes | IL.MethodAttributes.Virtual | IL.MethodAttributes.NewSlot | IL.MethodAttributes.Final
                : attributes;
        }

        /// <summary>
        /// The names of the methods of the interfaces a class implements.
        /// </summary>
        private HashSet<string> Implemented(ClassDefinition definition)
        {
            var names = new HashSet<string>();

            foreach (var @interface in definition.Interfaces)
                if (@interface is ClassDefinition own)
                    names.UnionWith([.. own.Methods.Select(method => method.Name), .. Implemented(own)]);
                else
                {
                    var type = ImportType(@interface);

                    foreach (var each in type.GetInterfaces().Append(type))
                        names.UnionWith(each.GetMethods().Select(method => method.Name));
                }

            return names;
        }

        /// <summary>
        /// A member as code refers to it. A member of a generic class is referred to
        /// through the class's self instance.
        /// </summary>
        internal IL.FieldInfo Referenced(ClassField field)
            => owners.TryGetValue(field, out var owner) && selves.TryGetValue(owner, out var self)
                ? Emit.TypeBuilder.GetField(self, classFields[field])
                : classFields[field];

        internal IL.MethodInfo Referenced(ClassMethod method)
            => owners.TryGetValue(method, out var owner) && selves.TryGetValue(owner, out var self)
                ? Emit.TypeBuilder.GetMethod(self, classMethods[method])
                : classMethods[method];

        internal IL.ConstructorInfo Referenced(ClassConstructor constructor)
            => owners.TryGetValue(constructor, out var owner) && selves.TryGetValue(owner, out var self)
                ? Emit.TypeBuilder.GetConstructor(self, classConstructors[constructor])
                : classConstructors[constructor];

        private IL.ConstructorInfo BaseConstructor(ClassDefinition definition, Type parent)
        {
            if (definition.Base is ClassDefinition @base)
                foreach (var constructor in @base.Constructors)
                    if (constructor.Signature.Parameters.Count == 0 && classConstructors.TryGetValue(constructor, out var builder))
                        return builder;

            return parent.GetConstructor(
                IL.BindingFlags.Instance | IL.BindingFlags.Public | IL.BindingFlags.NonPublic,
                Type.EmptyTypes
            ) ?? throw new InvalidOperationException($"{parent.Name} has no default constructor.");
        }

        /// <summary>
        /// The CLR needs a base class to exist before the classes deriving from it.
        /// </summary>
        private void CreateClass(ClassDefinition definition, Dictionary<ClassDefinition, Emit.TypeBuilder> builders)
        {
            if (classes[definition] is not Emit.TypeBuilder builder || !builders.ContainsKey(definition))
                return;

            if (definition.Base is ClassDefinition @base)
                CreateClass(@base, builders);

            foreach (var @interface in definition.Interfaces.OfType<ClassDefinition>())
                CreateClass(@interface, builders);

            classes[definition] = builder.CreateType();
        }

        /// <summary>
        /// The method a class method became, once its class is loaded.
        /// </summary>
        public IL.MethodBase Defined(ClassMethod method)
            => Created(classMethods[method]);

        /// <summary>
        /// The constructor a class constructor became, once its class is loaded.
        /// </summary>
        public IL.MethodBase Defined(ClassConstructor constructor)
            => Created(classConstructors[constructor]);

        private IL.MethodBase Created(IL.MethodBase builder)
            => classes.Values
                .First(type => type is not Emit.TypeBuilder && type.MetadataToken == builder.DeclaringType!.MetadataToken)
                .GetMembers(IL.BindingFlags.Public | IL.BindingFlags.NonPublic | IL.BindingFlags.Instance | IL.BindingFlags.Static | IL.BindingFlags.DeclaredOnly)
                .OfType<IL.MethodBase>()
                .First(member => member.MetadataToken == builder.MetadataToken);

        /// <summary>
        /// Gets the method of a loaded function.
        /// </summary>
        public IL.MethodInfo GetMethod(LIR.Function function)
            => functions.TryGetValue(function, out var method) && method is not Emit.MethodBuilder
                ? method
                : throw new InvalidOperationException($"Function {function.Name} is not loaded.");

        /// <summary>
        /// Gets the method of a function, loading its module first if nothing has.
        /// </summary>
        public IL.MethodInfo LoadMethod(LIR.Function function)
        {
            if (!(functions.TryGetValue(function, out var method) && method is not Emit.MethodBuilder))
                Load(function.Module ?? throw new InvalidOperationException($"Function {function.Name} has no module."));

            return GetMethod(function);
        }

        private Emit.TypeBuilder Holder(LIR.Module? owner)
        {
            if (owner is null)
                throw new InvalidOperationException("Only members of a module can be loaded.");

            if (!holders.TryGetValue(owner, out var holder))
                holders[owner] = holder = module.DefineType(
                    Unique(owner.Name),
                    IL.TypeAttributes.Public | IL.TypeAttributes.Abstract | IL.TypeAttributes.Sealed
                );

            return holder;
        }

        private Type DeclareStructure(LIR.Structure structure)
        {
            var type = module.DefineType(
                Unique(structure.Name),
                IL.TypeAttributes.Public | IL.TypeAttributes.Sealed | IL.TypeAttributes.SequentialLayout,
                typeof(ValueType)
            );

            // Recorded first, as a structure can refer to itself.
            structures[structure] = type;
            pendingStructures.Add(structure);

            foreach (var field in structure.Fields)
                fields[field] = type.DefineField(
                    Unique(field.Name),
                    ImportType(field.Type),
                    IL.FieldAttributes.Public
                );

            return type;
        }

        private IL.FieldInfo DeclareGlobal(LIR.Global global)
        {
            var field = Holder(global.Module).DefineField(
                Unique(global.Name),
                ImportType(global.Type),
                IL.FieldAttributes.Public | IL.FieldAttributes.Static
            );

            globals[global] = field;
            pendingGlobals.Add(global);

            return field;
        }

        private IL.MethodInfo DeclareFunction(LIR.Function function)
        {
            var method = Holder(function.Module).DefineMethod(
                Unique(function.Name),
                IL.MethodAttributes.Public | IL.MethodAttributes.Static,
                ImportType(function.Signature.ReturnType),
                [.. function.Signature.Parameters.Select(parameter => ImportType(parameter.Type))]
            );

            functions[function] = method;
            pendingFunctions.Add(function);

            bodies.Enqueue(() => new CodeEmitter(this, method.GetILGenerator(), function).EmitBody());

            return method;
        }

        private void Complete()
        {
            var completed = new HashSet<LIR.Structure>();

            foreach (var structure in pendingStructures)
                CompleteStructure(structure, completed);

            var types = holders.ToDictionary(holder => holder.Key, holder => holder.Value.CreateType());

            foreach (var global in pendingGlobals)
                globals[global] = types[global.Module!].GetField(globals[global].Name)!;

            foreach (var function in pendingFunctions)
                functions[function] = types[function.Module!].GetMethod(functions[function].Name)!;

            holders.Clear();
            pendingStructures.Clear();
            pendingGlobals.Clear();
            pendingFunctions.Clear();
        }

        /// <summary>
        /// The CLR needs an inline structure to exist before the one containing it.
        /// </summary>
        private void CompleteStructure(LIR.Structure structure, HashSet<LIR.Structure> completed)
        {
            if (structures[structure] is not Emit.TypeBuilder builder || !completed.Add(structure))
                return;

            foreach (var field in structure.Fields)
                if (field.Type is LIR.Structure inline)
                    CompleteStructure(inline, completed);

            var type = builder.CreateType();

            structures[structure] = type;

            foreach (var field in structure.Fields)
                fields[field] = type.GetField(fields[field].Name)!;
        }
    }
}
