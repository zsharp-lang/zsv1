namespace ZSharp.Platform.Runtime
{
    /// <summary>
    /// This platform's mark on a structure whose values are objects of a CLR class, or
    /// whose values are objects of any class that implements it, if it is an interface.
    /// What marks it is the capability of polymorphism the platform has.
    /// </summary>
    public sealed class Polymorphic
        : LIR.Metadata
    {
        public bool IsInterface { get; set; }

        /// <summary>
        /// The interfaces the class implements, or the interface extends.
        /// </summary>
        public List<LIR.Structure> Interfaces { get; } = [];
    }

    /// <summary>
    /// This platform's mark on a function that is a method of a polymorphic structure:
    /// it takes the object first, and calling it calls what the object's class has.
    /// </summary>
    public sealed class MethodOf(LIR.Structure owner)
        : LIR.Metadata
    {
        public LIR.Structure Owner { get; } = owner;
    }

    /// <summary>
    /// This platform's mark on a function with no code that casts an object down to a
    /// type it might be of, failing if it isn't. Calling it is the cast.
    /// </summary>
    public sealed class CastDown(LIR.Type target)
        : LIR.Metadata
    {
        public LIR.Type Target { get; } = target;
    }

    /// <summary>
    /// This platform's mark on a function with no code that tells whether an object is of
    /// a type. Calling it is the test, and <c>null</c> is of no type.
    /// </summary>
    public sealed class TypeTest(LIR.Type target)
        : LIR.Metadata
    {
        public LIR.Type Target { get; } = target;
    }

    partial class Runtime
    {
        /// <summary>
        /// Whether a function is an operation of the platform, which a call emits in place
        /// rather than calling anything.
        /// </summary>
        internal static bool IsOperation(LIR.Function function)
            => function.Metadata.Any(metadata => metadata is CastDown or TypeTest);

        private bool EmitOperation(Emit.ILGenerator il, LIR.Function function)
        {
            foreach (var metadata in function.Metadata)
                switch (metadata)
                {
                    case CastDown cast:
                        var type = ImportType(cast.Target);
                        il.Emit(type.IsValueType ? Emit.OpCodes.Unbox_Any : Emit.OpCodes.Castclass, type);
                        return true;
                    case TypeTest test:
                        il.Emit(Emit.OpCodes.Isinst, ImportType(test.Target));
                        il.Emit(Emit.OpCodes.Ldnull);
                        il.Emit(Emit.OpCodes.Cgt_Un);
                        return true;
                }

            return false;
        }

        private readonly Dictionary<LIR.Structure, ClassDefinition> polymorphicClasses = [];
        private readonly Dictionary<LIR.Structure, ClassConstructor> polymorphicConstructors = [];
        private readonly Dictionary<LIR.Field, ClassField> polymorphicFields = [];
        private readonly Dictionary<LIR.Function, ClassMethod> polymorphicMethods = [];

        internal static Polymorphic? PolymorphicOf(LIR.Structure structure)
            => structure.Metadata.OfType<Polymorphic>().FirstOrDefault();

        internal static bool IsPolymorphic(LIR.Type? type)
            => type is LIR.Structure structure && PolymorphicOf(structure) is not null;

        private static bool IsMethod(LIR.Function function)
            => function.Metadata.OfType<MethodOf>().Any();

        /// <summary>
        /// Realizes polymorphic structures, and the ones they reach, as one batch of CLR
        /// classes, so they can refer to each other.
        /// </summary>
        private void LoadPolymorphic(IEnumerable<LIR.Structure> structures)
        {
            var batch = new List<ClassDefinition>();

            foreach (var structure in structures)
                ClassOf(structure, batch);

            if (batch.Count > 0)
                Load(batch);
        }

        /// <summary>
        /// The class a polymorphic structure is, which is realized now if it isn't yet.
        /// </summary>
        private ClassDefinition ClassOf(LIR.Structure structure)
        {
            if (!polymorphicClasses.ContainsKey(structure))
                LoadPolymorphic([structure]);

            return polymorphicClasses[structure];
        }

        private ClassDefinition ClassOf(LIR.Structure structure, List<ClassDefinition> batch)
        {
            if (polymorphicClasses.TryGetValue(structure, out var known))
                return known;

            var polymorphic = PolymorphicOf(structure)!;
            var definition = new ClassDefinition(structure.Name ?? "Object") { IsInterface = polymorphic.IsInterface };

            polymorphicClasses[structure] = definition;
            batch.Add(definition);

            foreach (var @interface in polymorphic.Interfaces)
                definition.Interfaces.Add(ClassOf(@interface, batch));

            if (!polymorphic.IsInterface)
            {
                foreach (var (field, index) in structure.Fields.Select((field, index) => (field, index)))
                {
                    var classField = new ClassField(field.Name ?? $"field{index}", field.Type);

                    polymorphicFields[field] = classField;
                    definition.Fields.Add(classField);
                }

                // An object starts with no code of its own: a constructor of the class is
                // what the code that makes it calls after.
                var self = new LIR.Parameter(definition) { Name = "this" };
                var signature = new LIR.Signature();
                signature.Parameters.Add(self);

                var constructor = new ClassConstructor(new LIR.Signature(), new LIR.Function(signature) { Body = new LIR.Code() });

                polymorphicConstructors[structure] = constructor;
                definition.Constructors.Add(constructor);
            }

            foreach (var function in structure.Module?.Functions ?? Enumerable.Empty<LIR.Function>())
                if (function.Metadata.OfType<MethodOf>().Any(method => method.Owner == structure))
                {
                    // A function has a name only if it is given one, which the CLR needs.
                    var method = new ClassMethod(function.Name is { Length: > 0 } name ? name : $"<unnamed>{definition.Methods.Count}", isStatic: false, function) { IsAbstract = function.Body is null };

                    polymorphicMethods[function] = method;
                    definition.Methods.Add(method);
                }

            return definition;
        }

        internal bool TryMethod(LIR.Function function, out ClassMethod method)
            => polymorphicMethods.TryGetValue(function, out method!);

        internal ClassConstructor ConstructorOf(LIR.Structure structure)
        {
            ClassOf(structure);

            return polymorphicConstructors[structure];
        }

        internal IL.FieldInfo PolymorphicField(LIR.Structure structure, LIR.Field field)
        {
            ClassOf(structure);

            return Referenced(polymorphicFields[field]);
        }
    }
}
