using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Interface
    {
        public string? Name { get; set; }

        public InterfaceAttributes Attributes { get; set; } = InterfaceAttributes.None;

        public Collection<Interface> Bases { get; set; }

        public Collection<Method> Methods { get; }

        public Collection<Property> Properties { get; }

        //public Collection<Event> Events { get; }

        //public Collection<ZSObject> NestedTypes { get; }
    }
}
