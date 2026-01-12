using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Dataclass : TypeDefinition
    {
        public string? Name { get; set; }

        public DataclassAttributes Attributes { get; set; } = DataclassAttributes.None;

        public TypeReference<Dataclass>? Base { get; set; }

        public Collection<TypeReference<Typeclass>> TypeClasses { get; }

        public Collection<Constructor> Constructors { get; }
    }
}
