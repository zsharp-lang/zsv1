namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<LIR.Module, Emit.TypeBuilder> holders = [];
        private readonly Dictionary<ClassDefinition, Type> classes = [];
        private readonly Dictionary<ClassField, IL.FieldInfo> classFields = [];
        private readonly Dictionary<ClassConstructor, Emit.ConstructorBuilder> classConstructors = [];
        private readonly Dictionary<ClassMethod, Emit.MethodBuilder> classMethods = [];
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
                    IL.TypeAttributes.Public | IL.TypeAttributes.Class
                );

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

                foreach (var method in definition.Methods)
                    new CodeEmitter(this, classMethods[method].GetILGenerator(), method.Function).EmitBody();
            }

            foreach (var definition in builders.Keys)
                CreateClass(definition, builders);

            return [.. definitions.Select(definition => classes[definition])];
        }

        private void DeclareMembers(ClassDefinition definition, Emit.TypeBuilder builder)
        {
            builder.SetParent(definition.Base is null ? typeof(object) : ImportType(definition.Base));

            foreach (var field in definition.Fields)
                classFields[field] = builder.DefineField(field.Name, ImportType(field.Type), IL.FieldAttributes.Public);

            foreach (var constructor in definition.Constructors)
                classConstructors[constructor] = builder.DefineConstructor(
                    IL.MethodAttributes.Public,
                    IL.CallingConventions.Standard,
                    [.. constructor.Signature.Parameters.Select(parameter => ImportType(parameter.Type))]
                );

            foreach (var method in definition.Methods)
                classMethods[method] = builder.DefineMethod(
                    method.Name,
                    IL.MethodAttributes.Public | (method.IsStatic ? IL.MethodAttributes.Static : 0),
                    ImportType(method.Signature.ReturnType),
                    [.. method.Signature.Parameters.Skip(method.IsStatic ? 0 : 1).Select(parameter => ImportType(parameter.Type))]
                );
        }

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

            classes[definition] = builder.CreateType();
        }

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
