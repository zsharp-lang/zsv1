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
    }

    public sealed class ClassField(string name, LIR.Type type)
    {
        public string Name { get; set; } = name;

        public LIR.Type Type { get; set; } = type;
    }
}
