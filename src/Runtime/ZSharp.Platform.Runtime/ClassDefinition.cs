namespace ZSharp.Platform.Runtime
{
    /// <summary>
    /// A CLR class, as LIR that only this platform handles. LIR has no classes, and
    /// no other target ever handles a CLR class definition, so this belongs here and
    /// not in the LIR core.
    /// </summary>
    public sealed class ClassDefinition(string name)
        : LIR.Type
    {
        public string Name { get; set; } = name;

        public LIR.Type? Base { get; set; }

        /// <summary>
        /// Whether it is an interface: it has no base, fields or constructors, and its
        /// methods are abstract.
        /// </summary>
        public bool IsInterface { get; set; }

        /// <summary>
        /// The interfaces it implements, or extends if it is one.
        /// </summary>
        public List<LIR.Type> Interfaces { get; } = [];

        /// <summary>
        /// The type parameters of a generic class, which its members' types can use.
        /// </summary>
        public List<ClassGenericParameter> GenericParameters { get; } = [];

        public List<ClassField> Fields { get; } = [];

        public List<ClassConstructor> Constructors { get; } = [];

        public List<ClassMethod> Methods { get; } = [];
    }

    /// <summary>
    /// A type parameter of a generic class, as a type its members use.
    /// </summary>
    public sealed class ClassGenericParameter(string name)
        : LIR.Type
    {
        public string Name { get; set; } = name;
    }

    public sealed class ClassField(string name, LIR.Type type)
    {
        public string Name { get; set; } = name;

        public LIR.Type Type { get; set; } = type;
    }

    /// <summary>
    /// Reading or writing a field, as something code can call.
    /// </summary>
    public sealed class ClassFieldAccess(ClassField field, bool isSet, LIR.Signature signature)
        : LIR.ICallable
    {
        public ClassField Field { get; } = field;

        public bool IsSet { get; } = isSet;

        public LIR.Signature Signature { get; } = signature;
    }

    /// <summary>
    /// Calling it creates an object. <see cref="Function"/> is its code, which takes
    /// that object first.
    /// </summary>
    public sealed class ClassConstructor(LIR.Signature signature, LIR.Function function)
        : LIR.ICallable
    {
        public LIR.Signature Signature { get; } = signature;

        public LIR.Function Function { get; } = function;
    }

    /// <summary>
    /// A method. An instance method's function takes its object first.
    /// </summary>
    public sealed class ClassMethod(string name, bool isStatic, LIR.Function function)
        : LIR.ICallable
    {
        public string Name { get; set; } = name;

        public bool IsStatic { get; } = isStatic;

        public bool IsVirtual { get; set; }

        /// <summary>
        /// The type parameters of a generic method, which its signature and code use.
        /// </summary>
        public List<ClassGenericParameter> GenericParameters { get; } = [];

        /// <summary>
        /// Whether it has no code, so a derived class must override it.
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Whether it overrides a virtual method of the base, rather than starting a new one.
        /// </summary>
        public bool IsOverride { get; set; }

        public LIR.Function Function { get; } = function;

        public LIR.Signature Signature => Function.Signature;
    }
}
