using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Dataclass : OOPType
    {
        public string? Name { get; set; }

        public DataclassAttributes Attributes { get; set; } = DataclassAttributes.None;

        public OOPTypeReference<Dataclass>? Base { get; set; }

        public Collection<OOPTypeReference<Typeclass>> TypeClasses { get; }

        public Collection<Constructor> Constructors { get; }
    }
}
