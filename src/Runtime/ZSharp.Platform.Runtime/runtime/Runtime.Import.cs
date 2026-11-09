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
                ClrType clr => ImportClrType(clr),
                ClrArrayType array => array.IsVector ? ImportType(array.Element).MakeArrayType() : ImportType(array.Element).MakeArrayType(array.Rank),
                ClrPointerType pointer => ImportType(pointer.Target).MakePointerType(),
                LIR.Structure structure when PolymorphicOf(structure) is not null => ImportType(ClassOf(structure)),
                LIR.Structure structure => ImportStructure(structure),
                // A generic class still being defined is its self instance to its own code.
                ClassDefinition definition => Realized(definition) is var @class
                    && @class is Emit.TypeBuilder && selves.TryGetValue(definition, out var self) ? self : @class,
                ClassGenericParameter parameter => genericParameters.TryGetValue(parameter, out var argument)
                    ? argument
                    : throw new InvalidOperationException($"Type parameter {parameter.Name} is not defined."),
                LIR.Reference reference => typeof(Box<>).MakeGenericType(ImportType(reference.Target)),
                LIR.GenericInstance instance => ImportType(instance.Definition).MakeGenericType([.. instance.Arguments.Select(ImportType)]),
                ByRefType reference => ImportType(reference.Target).MakeByRefType(),
                LIR.FunctionType => typeof(nint),
                _ => throw new NotSupportedException($"Can't import a {type.GetType().Name}.")
            };

        internal IL.FieldInfo ImportField(LIR.Structure structure, LIR.Field field)
        {
            if (PolymorphicOf(structure) is not null)
                return PolymorphicField(structure, field);

            ImportStructure(structure);

            return fields[field];
        }

        /// <summary>
        /// A module's globals are declared together, in their order, so its initializers
        /// run in that order.
        /// </summary>
        internal IL.FieldInfo ImportGlobal(LIR.Global global)
        {
            if (!globals.TryGetValue(global, out var result))
            {
                foreach (var declared in global.Module?.Globals ?? Enumerable.Empty<LIR.Global>())
                    if (!globals.ContainsKey(declared))
                        DeclareGlobal(declared);

                if (global.Module is { } module)
                    Initialize(module);

                result = globals.TryGetValue(global, out var found) ? found : DeclareGlobal(global);
            }

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

        /// <summary>
        /// Whether a type is one still being built, or an instance of one, or of anything
        /// over one, whose members are found through the builder's.
        /// </summary>
        private static bool ContainsBuilder(Type type)
            => type is Emit.TypeBuilder
            || (type.IsGenericType && !type.IsGenericTypeDefinition
                && (type.GetGenericTypeDefinition() is Emit.TypeBuilder || type.GetGenericArguments().Any(ContainsBuilder)));

        private Type ImportStructure(LIR.Structure structure)
        {
            if (!structures.TryGetValue(structure, out var result))
                result = DeclareStructure(structure);

            return result;
        }
    }
}
