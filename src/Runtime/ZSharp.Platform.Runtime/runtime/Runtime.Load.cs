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

        /// <summary>
        /// The modules whose initializer was given to a holder, and those whose holder's
        /// type initializer is still to be emitted.
        /// </summary>
        private readonly HashSet<LIR.Module> initializedModules = [];
        private readonly List<LIR.Module> pendingInitializers = [];

        /// <summary>
        /// A module's initializer runs as the type initializer of the holder its globals
        /// are declared in, which the CLR runs before any of them is first used.
        /// </summary>
        internal void Initialize(LIR.Module module)
        {
            if (module.Initializer.Block.Statements.Count > 0 && initializedModules.Add(module))
                pendingInitializers.Add(module);
        }
        private readonly List<LIR.Function> pendingFunctions = [];

        /// <summary>
        /// Loads a module, and whatever it reaches in other modules.
        /// </summary>
        public void Load(LIR.Module module)
            => Loading(() => { LoadModule(module); return true; });

        private void LoadModule(LIR.Module module)
        {
            // Polymorphic structures are classes, realized together with their methods.
            LoadPolymorphic(module.Structures.Where(structure => PolymorphicOf(structure) is not null));

            foreach (var structure in module.Structures.Where(structure => PolymorphicOf(structure) is null))
                ImportStructure(structure);

            foreach (var global in module.Globals)
                ImportGlobal(global);

            foreach (var function in module.Functions.Where(function => !IsMethod(function)))
                ImportFunction(function);

            while (bodies.TryDequeue(out var body))
                body();

            Complete();
        }

        /// <summary>
        /// The type a class definition is, realized the first time it is reached, with the
        /// classes its declarations reach, so none of them reaches another while its members
        /// are being declared. A class reached by code is loaded inside the load of that
        /// code, whose classes are declared by then.
        /// </summary>
        private Type Realized(ClassDefinition definition)
        {
            if (!classes.TryGetValue(definition, out var known))
            {
                var batch = new List<ClassDefinition>();

                Reach(definition, batch);
                Loading(() => LoadClasses(batch));
                known = classes[definition];
            }

            return known;
        }

        private bool Realized(ClassMember member)
        {
            if (member.Owner is { } owner)
                Realized(owner);

            return true;
        }

        private void Reach(ClassDefinition definition, List<ClassDefinition> batch)
        {
            if (classes.ContainsKey(definition) || batch.Contains(definition))
                return;

            batch.Add(definition);

            // A nested class is defined with the class it is in, and that class with it.
            if (definition.DeclaringType is { } outer)
                Reach(outer, batch);

            foreach (var nested in definition.NestedTypes)
                Reach(nested, batch);

            Reach(definition.Base, batch);

            foreach (var @interface in definition.Interfaces)
                Reach(@interface, batch);

            foreach (var field in definition.Fields)
                Reach(field.Type, batch);

            foreach (var property in definition.Properties)
                Reach(property.Type, batch);

            foreach (var signature in definition.Constructors.Select(constructor => constructor.Signature).Concat(definition.Methods.Select(method => method.Signature)))
            {
                Reach(signature.ReturnType, batch);

                foreach (var parameter in signature.Parameters)
                    Reach(parameter.Type, batch);
            }
        }

        private void Reach(LIR.Type? type, List<ClassDefinition> batch)
        {
            switch (type)
            {
                case ClassDefinition definition: Reach(definition, batch); break;
                case LIR.GenericInstance instance:
                    Reach(instance.Definition, batch);
                    foreach (var argument in instance.Arguments)
                        Reach(argument, batch);
                    break;
                case ByRefType reference: Reach(reference.Target, batch); break;
                case ClrArrayType array: Reach(array.Element, batch); break;
                case ClrPointerType pointer: Reach(pointer.Target, batch); break;
                case LIR.Reference reference: Reach(reference.Target, batch); break;
            }
        }

        /// <summary>
        /// Loads classes that may refer to each other and to themselves.
        /// </summary>
        public Type[] Load(IReadOnlyList<ClassDefinition> definitions)
            => Loading(() => definitions.Select(Realized).ToArray());

        private Type[] LoadClasses(IReadOnlyList<ClassDefinition> definitions)
        {
            classLoading++;

            try
            {
                return DefineClasses(definitions);
            }
            finally
            {
                if (--classLoading == 0)
                    CreateClasses();
            }
        }

        /// <summary>
        /// How many class loads are running, one inside another's code.
        /// </summary>
        private int classLoading;

        private void CreateClasses()
        {
            foreach (var definition in uncreated.ToList())
                CreateClass(definition);
        }

        private Type[] DefineClasses(IReadOnlyList<ClassDefinition> definitions)
        {
            var builders = new Dictionary<ClassDefinition, Emit.TypeBuilder>();

            // Everything is declared first, as any of them can refer to any other: a class
            // nested in another after it, through it.
            foreach (var definition in definitions.Where(definition => !classes.ContainsKey(definition)).OrderBy(Depth))
            {
                var attributes = definition.IsInterface
                    ? Visibility(definition) | IL.TypeAttributes.Interface | IL.TypeAttributes.Abstract
                    : Visibility(definition) | IL.TypeAttributes.Class
                        | (definition.Methods.Any(method => method.IsAbstract) ? IL.TypeAttributes.Abstract : 0);

                classes[definition] = builders[definition] = definition.DeclaringType is { } outer
                    ? (classes[outer] as Emit.TypeBuilder ?? throw new NotSupportedException($"Class {definition.Name} is nested in {outer.Name}, which is already created."))
                        .DefineNestedType(definition.Name, attributes)
                    : module.DefineType(classNames.Add(definition.Name) ? definition.Name : Unique(definition.Name), attributes);
            }

            foreach (var (definition, builder) in builders.Where(pair => pair.Key.GenericParameters.Count > 0))
            {
                var parameters = builder.DefineGenericParameters([.. definition.GenericParameters.Select(parameter => parameter.Name)]);

                foreach (var (parameter, type) in definition.GenericParameters.Zip(parameters))
                    genericParameters[parameter] = type;

                selves[definition] = builder.MakeGenericType(parameters);
            }

            foreach (var (definition, builder) in builders)
                DeclareMembers(definition, builder);

            // Once every member is declared, a method that implements an interface's but
            // isn't public says which one it implements, as the CLR's explicit
            // implementations do.
            foreach (var (definition, builder) in builders)
                DefineImplementations(definition, builder);

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

            // A class is created once the outermost class load is complete, as a class whose
            // code reached it may still be building what it derives from.
            foreach (var definition in builders.Keys)
                uncreated.Add(definition);

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
                classFields[field] = builder.DefineField(field.Name, ImportType(field.Type), FieldVisibility(Accessible.Of(field.Metadata)));

            foreach (var member in definition.Fields.Cast<object>().Concat(definition.Constructors).Concat(definition.Methods))
                owners[member] = definition;

            foreach (var constructor in definition.Constructors)
                classConstructors[constructor] = builder.DefineConstructor(
                    MethodVisibility(Accessible.Of(constructor.Metadata)),
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

            foreach (var property in definition.Properties)
            {
                var declared = builder.DefineProperty(
                    property.Name,
                    IL.PropertyAttributes.None,
                    property.IsStatic ? IL.CallingConventions.Standard : IL.CallingConventions.HasThis,
                    ImportType(property.Type),
                    Type.EmptyTypes
                );

                if (property.Getter is { } getter)
                    declared.SetGetMethod(classMethods[getter]);

                if (property.Setter is { } setter)
                    declared.SetSetMethod(classMethods[setter]);
            }
        }

        /// <summary>
        /// How a method is declared. A method that implements an interface method by its
        /// name is virtual and final, as C# makes it, so the CLR maps it to the interface's.
        /// </summary>
        private static IL.MethodAttributes Attributes(ClassDefinition definition, ClassMethod method, HashSet<string> implemented)
        {
            var attributes = MethodVisibility(Accessible.Of(method.Metadata)) | IL.MethodAttributes.HideBySig;

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
        /// A type's visibility: a top-level type is public or else internal to its assembly.
        /// </summary>
        private static IL.TypeAttributes Visibility(ClassDefinition definition)
            => definition.DeclaringType is null
                ? Accessible.Of(definition.Metadata) == Accessibility.Public ? IL.TypeAttributes.Public : IL.TypeAttributes.NotPublic
                : Accessible.Of(definition.Metadata) switch
                {
                    Accessibility.Public => IL.TypeAttributes.NestedPublic,
                    Accessibility.Private => IL.TypeAttributes.NestedPrivate,
                    Accessibility.Internal => IL.TypeAttributes.NestedAssembly,
                    Accessibility.Protected => IL.TypeAttributes.NestedFamily,
                    Accessibility.ProtectedInternal => IL.TypeAttributes.NestedFamORAssem,
                    Accessibility.PrivateProtected => IL.TypeAttributes.NestedFamANDAssem,
                    _ => throw new ArgumentOutOfRangeException(nameof(definition)),
                };

        /// <summary>
        /// How deep a class is nested: a class is defined after the one it is in.
        /// </summary>
        private static int Depth(ClassDefinition definition)
            => definition.DeclaringType is { } outer ? Depth(outer) + 1 : 0;

        private static IL.MethodAttributes MethodVisibility(Accessibility accessibility)
            => accessibility switch
            {
                Accessibility.Public => IL.MethodAttributes.Public,
                Accessibility.Private => IL.MethodAttributes.Private,
                Accessibility.Internal => IL.MethodAttributes.Assembly,
                Accessibility.Protected => IL.MethodAttributes.Family,
                Accessibility.ProtectedInternal => IL.MethodAttributes.FamORAssem,
                Accessibility.PrivateProtected => IL.MethodAttributes.FamANDAssem,
                _ => throw new ArgumentOutOfRangeException(nameof(accessibility)),
            };

        private static IL.FieldAttributes FieldVisibility(Accessibility accessibility)
            => accessibility switch
            {
                Accessibility.Public => IL.FieldAttributes.Public,
                Accessibility.Private => IL.FieldAttributes.Private,
                Accessibility.Internal => IL.FieldAttributes.Assembly,
                Accessibility.Protected => IL.FieldAttributes.Family,
                Accessibility.ProtectedInternal => IL.FieldAttributes.FamORAssem,
                Accessibility.PrivateProtected => IL.FieldAttributes.FamANDAssem,
                _ => throw new ArgumentOutOfRangeException(nameof(accessibility)),
            };

        /// <summary>
        /// Says which interface method each method of a class that isn't public implements,
        /// as the CLR maps only a public one by its name: one of its name and number of
        /// parameters, of each interface the class implements.
        /// </summary>
        private void DefineImplementations(ClassDefinition definition, Emit.TypeBuilder builder)
        {
            if (definition.IsInterface)
                return;

            foreach (var method in definition.Methods.Where(method => !method.IsStatic && Accessible.Of(method.Metadata) != Accessibility.Public))
                foreach (var @interface in definition.Interfaces)
                    foreach (var (name, taken, target) in InterfaceMethods(@interface))
                        if (name == method.Name && taken == method.Signature.Parameters.Count - 1)
                            builder.DefineMethodOverride(classMethods[method], target);
        }

        /// <summary>
        /// The methods of an interface and of the interfaces it extends, as a class that
        /// implements it refers to them, with their names and how many values they take. An
        /// interface defined with the class isn't created yet, so its methods say what they
        /// take as LIR does.
        /// </summary>
        private IEnumerable<(string Name, int Taken, IL.MethodInfo Method)> InterfaceMethods(LIR.Type @interface)
        {
            if (@interface is ClassDefinition own)
                return own.Methods
                    .Select(method => (method.Name, method.Signature.Parameters.Count - 1, (IL.MethodInfo)classMethods[method]))
                    .Concat(own.Interfaces.SelectMany(InterfaceMethods));

            if (@interface is LIR.GenericInstance { Definition: ClassDefinition generic })
            {
                var instance = ImportType(@interface);
                return generic.Methods.Select(method => (method.Name, method.Signature.Parameters.Count - 1, Emit.TypeBuilder.GetMethod(instance, classMethods[method])));
            }

            var type = ImportType(@interface);
            return type.GetInterfaces().Append(type).SelectMany(each => each.GetMethods()).Select(method => (method.Name, method.GetParameters().Length, method));
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
        /// through the class's self instance. Once its class exists, a member is the one
        /// the class has, which any code can refer to, a standalone function's too.
        /// </summary>
        internal IL.FieldInfo Referenced(ClassField field)
            => Realized(field) && owners.TryGetValue(field, out var owner) && selves.TryGetValue(owner, out var self)
                ? Emit.TypeBuilder.GetField(self, classFields[field])
                : IsCreated(field)
                ? CreatedField(classFields[field])
                : classFields[field];

        internal IL.MethodInfo Referenced(ClassMethod method)
            => Realized(method) && owners.TryGetValue(method, out var owner) && selves.TryGetValue(owner, out var self)
                ? Emit.TypeBuilder.GetMethod(self, classMethods[method])
                : IsCreated(method)
                ? (IL.MethodInfo)Created(classMethods[method])
                : classMethods[method];

        internal IL.ConstructorInfo Referenced(ClassConstructor constructor)
            => Realized(constructor) && owners.TryGetValue(constructor, out var owner) && selves.TryGetValue(owner, out var self)
                ? Emit.TypeBuilder.GetConstructor(self, classConstructors[constructor])
                : IsCreated(constructor)
                ? (IL.ConstructorInfo)Created(classConstructors[constructor])
                : classConstructors[constructor];

        /// <summary>
        /// A member as its class declares it, rather than as the class's own code refers to
        /// it, which a member of an instance of the class is found from.
        /// </summary>
        internal IL.MethodInfo Declared(ClassMethod method)
            => Realized(method) && IsCreated(method) ? (IL.MethodInfo)Created(classMethods[method]) : classMethods[method];

        internal IL.ConstructorInfo Declared(ClassConstructor constructor)
            => Realized(constructor) && IsCreated(constructor) ? (IL.ConstructorInfo)Created(classConstructors[constructor]) : classConstructors[constructor];

        /// <summary>
        /// A field of a class defined here, on an instance of the class.
        /// </summary>
        internal IL.FieldInfo FieldOnInstance(ClassField field, Type instance)
        {
            Realized(field);

            var declared = IsCreated(field) ? CreatedField(classFields[field]) : classFields[field];

            // A field of a constructed type has its definition's token.
            return ContainsBuilder(instance)
                ? Emit.TypeBuilder.GetField(instance, declared)
                : instance
                    .GetFields(IL.BindingFlags.Public | IL.BindingFlags.NonPublic | IL.BindingFlags.Instance | IL.BindingFlags.Static | IL.BindingFlags.DeclaredOnly)
                    .First(candidate => candidate.MetadataToken == declared.MetadataToken);
        }

        private bool IsCreated(object member)
            => owners.TryGetValue(member, out var owner) && classes.TryGetValue(owner, out var type) && type is not Emit.TypeBuilder;

        private IL.FieldInfo CreatedField(IL.FieldInfo builder)
            => classes.Values
                .First(type => type is not Emit.TypeBuilder && type.MetadataToken == builder.DeclaringType!.MetadataToken)
                .GetFields(IL.BindingFlags.Public | IL.BindingFlags.NonPublic | IL.BindingFlags.Instance | IL.BindingFlags.Static | IL.BindingFlags.DeclaredOnly)
                .First(member => member.MetadataToken == builder.MetadataToken);

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
        private void CreateClass(ClassDefinition definition)
        {
            if (classes[definition] is not Emit.TypeBuilder builder || !uncreated.Remove(definition))
                return;

            // The CLR creates a nested class after the class it is in.
            if (definition.DeclaringType is { } outer)
                CreateClass(outer);

            if (definition.Base is ClassDefinition @base)
                CreateClass(@base);

            foreach (var @interface in definition.Interfaces.OfType<ClassDefinition>())
                CreateClass(@interface);

            classes[definition] = builder.CreateType();
        }

        /// <summary>
        /// The classes declared and emitted, but not created yet.
        /// </summary>
        private readonly HashSet<ClassDefinition> uncreated = [];

        /// <summary>
        /// The class definition a type this runtime made is, if it is one.
        /// </summary>
        public ClassDefinition? DefinitionOf(Type type)
        {
            foreach (var (definition, made) in classes)
                if (made == type)
                    return definition;

            return null;
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
            if (function.Module is null)
                return Loading(() => ImportFunction(function));

            if (!(functions.TryGetValue(function, out var method) && method is not Emit.MethodBuilder))
                Load(function.Module);

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
                FieldVisibility(Accessible.Of(global.Metadata)) | IL.FieldAttributes.Static
            );

            globals[global] = field;
            pendingGlobals.Add(global);

            return field;
        }

        private IL.MethodInfo DeclareFunction(LIR.Function function)
        {
            if (function.Module is null)
                return DeclareStandalone(function);

            var method = Holder(function.Module).DefineMethod(
                Unique(function.Name),
                MethodVisibility(Accessible.Of(function.Metadata)) | IL.MethodAttributes.Static,
                ImportType(function.Signature.ReturnType),
                [.. function.Signature.Parameters.Select(parameter => ImportType(parameter.Type))]
            );

            functions[function] = method;
            pendingFunctions.Add(function);

            bodies.Enqueue(() => new CodeEmitter(this, method.GetILGenerator(), function).EmitBody());

            return method;
        }

        /// <summary>
        /// Emits the type initializers of the holders whose modules have initializers.
        /// What an initializer reaches is declared and emitted with it.
        /// </summary>
        private void EmitInitializers()
        {
            while (pendingInitializers.Count > 0)
            {
                var pending = pendingInitializers.ToList();
                pendingInitializers.Clear();

                foreach (var owner in pending)
                {
                    var initializer = new LIR.Function(new LIR.Signature()) { Name = ".cctor", Body = owner.Initializer };

                    new CodeEmitter(this, Holder(owner).DefineTypeInitializer().GetILGenerator(), initializer).EmitBody();
                }

                while (bodies.TryDequeue(out var body))
                    body();
            }
        }

        private void Complete()
        {
            EmitInitializers();

            if (classLoading == 0)
                CreateClasses();

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
