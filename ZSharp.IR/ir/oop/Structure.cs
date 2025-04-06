using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Structure : OOPType
    {
        public string? Name { get; set; }

        public StructureAttributes Attributes { get; set; } = StructureAttributes.None;

        public Collection<OOPTypeReference<Structure>> Bases { get; set; }

        public Collection<Method> Methods { get; }

        public Collection<Property> Properties { get; }

        //public Collection<Event> Events { get; }

        //public Collection<ZSObject> NestedTypes { get; }
    }
}
