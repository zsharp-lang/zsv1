namespace ZSharp.IR
{
    public sealed class FieldReference(Field field)
    {
        public Field Field { get; set; } = field;

        public required OOPTypeReference OwningType { get; set; }
    }
}
