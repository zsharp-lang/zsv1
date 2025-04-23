namespace ZSharp.IR
{
    public sealed class ClassReference(Class @class)
        : OOPTypeReference<Class>
    {
        public Class Definition { get; } = @class;

        public OOPTypeReference? OwningType { get; set; }
    }
}
