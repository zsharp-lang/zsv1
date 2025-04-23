namespace ZSharp.IR
{
    public sealed class InterfaceReference(Interface @interface)
        : OOPTypeReference<Interface>
    {
        public Interface Definition { get; } = @interface;

        public OOPTypeReference? OwningType { get; set; }
    }
}
