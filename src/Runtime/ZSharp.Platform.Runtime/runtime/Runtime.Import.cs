namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<LIR.Structure, Type> structures = [];
        private readonly Dictionary<LIR.Field, IL.FieldInfo> fields = [];
        private readonly Dictionary<LIR.Global, IL.FieldInfo> globals = [];
        private readonly Dictionary<LIR.Function, IL.MethodInfo> functions = [];

        internal Type ImportType(LIR.Type? type)
            => type switch
            {
                null => typeof(void),
                LIR.TypeHandle handle => typeHandles.TryGetValue(handle, out var mapped)
                    ? mapped
                    : throw new InvalidOperationException("A type handle is not mapped."),
                LIR.Structure structure => ImportStructure(structure),
                ClassDefinition definition => classes.TryGetValue(definition, out var @class)
                    ? @class
                    : throw new InvalidOperationException($"Class {definition.Name} is not loaded."),
                LIR.Reference reference => typeof(Box<>).MakeGenericType(ImportType(reference.Target)),
                LIR.FunctionType => typeof(nint),
                _ => throw new NotSupportedException($"Can't import a {type.GetType().Name}.")
            };

        internal IL.FieldInfo ImportField(LIR.Structure structure, LIR.Field field)
        {
            ImportStructure(structure);

            return fields[field];
        }

        internal IL.FieldInfo ImportGlobal(LIR.Global global)
        {
            if (!globals.TryGetValue(global, out var result))
                result = DeclareGlobal(global);

            return result;
        }

        internal IL.MethodInfo ImportFunction(LIR.Function function)
        {
            if (!functions.TryGetValue(function, out var result))
                result = DeclareFunction(function);

            return result;
        }

        internal static IL.FieldInfo BoxValue(Type box)
            => ContainsBuilder(box)
                ? Emit.TypeBuilder.GetField(box, typeof(Box<>).GetField(nameof(Box<>.Value))!)
                : box.GetField(nameof(Box<>.Value))!;

        internal static IL.ConstructorInfo BoxConstructor(Type box)
            => ContainsBuilder(box)
                ? Emit.TypeBuilder.GetConstructor(box, typeof(Box<>).GetConstructors()[0])
                : box.GetConstructors()[0];

        private static bool ContainsBuilder(Type type)
            => type is Emit.TypeBuilder
            || (type.IsGenericType && type.GetGenericArguments().Any(ContainsBuilder));

        private Type ImportStructure(LIR.Structure structure)
        {
            if (!structures.TryGetValue(structure, out var result))
                result = DeclareStructure(structure);

            return result;
        }
    }
}
