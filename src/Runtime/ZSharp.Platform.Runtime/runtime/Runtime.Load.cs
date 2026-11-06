namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<LIR.Module, Emit.TypeBuilder> holders = [];
        private readonly Dictionary<ClassDefinition, Type> classes = [];
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
        /// Loads a class, which may refer to itself.
        /// </summary>
        public Type Load(ClassDefinition definition)
        {
            if (classes.TryGetValue(definition, out var loaded))
                return loaded;

            // Recorded first, as a class can refer to itself.
            var builder = module.DefineType(
                classNames.Add(definition.Name) ? definition.Name : Unique(definition.Name),
                IL.TypeAttributes.Public | IL.TypeAttributes.Class
            );

            classes[definition] = builder;

            if (definition.Base is not null)
                builder.SetParent(ImportType(definition.Base));

            foreach (var field in definition.Fields)
                builder.DefineField(field.Name, ImportType(field.Type), IL.FieldAttributes.Public);

            builder.DefineDefaultConstructor(IL.MethodAttributes.Public);

            return classes[definition] = builder.CreateType();
        }

        /// <summary>
        /// Gets the method of a loaded function.
        /// </summary>
        public IL.MethodInfo GetMethod(LIR.Function function)
            => functions.TryGetValue(function, out var method) && method is not Emit.MethodBuilder
                ? method
                : throw new InvalidOperationException($"Function {function.Name} is not loaded.");

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
