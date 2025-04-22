namespace ZSharp.IR
{
    public sealed class FieldReference(Field field)
    {
        public Field Member { get; set; } = field;

        public required OOPTypeReference OwningType { get; set; }
    }
}
