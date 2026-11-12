namespace ZSharp.LIR.CLR
{
    /// <summary>
    /// A CLR class, as LIR that the CLR platforms handle. LIR has no classes, and
    /// only a CLR platform handles a CLR class definition, so this belongs with the
    /// CLR handles and not in the LIR core.
    /// </summary>
    public sealed class ClassDefinition(string name)
        : LIR.Type
    {
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        public string Name { get; set; } = name;

        /// <summary>
        /// What a platform may interpret of the class, as the annotations kept of what it
        /// was lowered from.
        /// </summary>
        public List<Metadata> Metadata { get; } = [];

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

        public ClassMembers<ClassField> Fields => fields ??= new(this);

        public ClassMembers<ClassConstructor> Constructors => constructors ??= new(this);

        public ClassMembers<ClassMethod> Methods => methods ??= new(this);

        private ClassMembers<ClassField>? fields;
        private ClassMembers<ClassConstructor>? constructors;
        private ClassMembers<ClassMethod>? methods;

        public List<ClassProperty> Properties { get; } = [];
    }

    /// <summary>
    /// A property of a class: the methods that read and write it, which the CLR links by
    /// its name, so reflection and C# see a property.
    /// </summary>
    public sealed class ClassProperty(string name, LIR.Type type, bool isStatic)
    {
        public string Name { get; set; } = name;

        public LIR.Type Type { get; set; } = type;

        public bool IsStatic { get; } = isStatic;

        public ClassMethod? Getter { get; set; }

        public ClassMethod? Setter { get; set; }
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
        : ClassMember
    {
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        public ClassDefinition? Owner { get; set; }

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
        , ClassMember
    {
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        public ClassDefinition? Owner { get; set; }

        public LIR.Signature Signature { get; } = signature;

        public LIR.Function Function { get; } = function;
    }

    /// <summary>
    /// A method. An instance method's function takes its object first.
    /// </summary>
    public sealed class ClassMethod(string name, bool isStatic, LIR.Function function)
        : LIR.ICallable
        , ClassMember
    {
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        public ClassDefinition? Owner { get; set; }

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

    /// <summary>
    /// A field, constructor or method of a class, which knows the class it is in, so a
    /// platform reaching a member first reaches its class.
    /// </summary>
    public interface ClassMember
    {
        public ClassDefinition? Owner { get; set; }
    }

    /// <summary>
    /// The members of a class of one kind. Adding one makes the class its owner.
    /// </summary>
    public sealed class ClassMembers<T>(ClassDefinition owner)
        : System.Collections.ObjectModel.Collection<T>
        where T : ClassMember
    {
        protected override void InsertItem(int index, T item)
        {
            item.Owner = owner;
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            item.Owner = owner;
            base.SetItem(index, item);
        }
    }

    /// <summary>
    /// A CLR platform's mark on a module that defines a class, which is the module's as its
    /// functions are: a platform that writes the module writes the class with it.
    /// </summary>
    public sealed class DefinesClass(ClassDefinition definition)
        : LIR.Metadata
    {
        public ClassDefinition Definition { get; } = definition;
    }
}
