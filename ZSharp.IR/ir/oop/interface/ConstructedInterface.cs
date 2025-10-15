using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class ConstructedInterface(Interface @interface)
        : ConstructedType<Interface>
    {
        public Interface Interface { get; set; } = @interface;

        Interface TypeReference<Interface>.Definition => Interface;

        public TypeReference? OwningType { get; set; }

        public Collection<IType> Arguments { get; set; } = @interface.HasGenericParameters ? [] : Collection<IType>.Empty;
    }
}
