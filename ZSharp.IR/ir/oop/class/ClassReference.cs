namespace ZSharp.IR
{
    public sealed class ClassReference(Class @class)
        : TypeReference<Class>
    {
        public Class Definition { get; } = @class;

        public TypeReference? OwningType { get; set; }
    }
}
