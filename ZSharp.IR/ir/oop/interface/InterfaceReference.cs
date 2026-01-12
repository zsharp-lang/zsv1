namespace ZSharp.IR
{
    public sealed class InterfaceReference(Interface @interface)
        : TypeReference<Interface>
    {
        public Interface Definition { get; } = @interface;

        public TypeReference? OwningType { get; set; }
    }
}
