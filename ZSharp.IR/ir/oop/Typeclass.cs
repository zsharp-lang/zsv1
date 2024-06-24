using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Typeclass
    {
        public string? Name { get; set; }

        public TypeclassAttributes Attributes { get; set; } = TypeclassAttributes.None;

        public Collection<Typeclass> Bases { get; }

        public TemplateParameter Parameter { get; }

        public Collection<Method> Methods { get; }

        public Collection<Property> Properties { get; }

        //public Collection<ZSObject> NestedTypes { get; }
    }
}
