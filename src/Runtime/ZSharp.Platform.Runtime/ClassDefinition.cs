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

        public List<ClassField> Fields { get; } = [];

        public List<ClassConstructor> Constructors { get; } = [];

        public List<ClassMethod> Methods { get; } = [];
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

        public LIR.Function Function { get; } = function;

        public LIR.Signature Signature => Function.Signature;
    }
}
