using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Dataclass
    {
        public string? Name { get; set; }

        public DataclassAttributes Attributes { get; set; } = DataclassAttributes.None;

        public Dataclass? Base { get; set; }

        public Collection<Typeclass> TypeClasses { get; }

        public Collection<Constructor> Constructors { get; }
    }
}
