using CommonZ.Utils;

namespace ZSharp.IR
{
    public class Class(string? name) : OOPType
    {
        public string? Name { get; set; } = name;

        public ClassAttributes Attributes { get; set; } = ClassAttributes.None;

        public Class? Base { get; set; }

        public Class(string? name, Class? @class)
            : this(name)
        {
            Base = @class;
        }

        public Collection<Interface> Interfaces { get; } = [];

        public Collection<Field> Fields { get; } = [];

        public Collection<Method> Methods { get; } = [];

        public Collection<Property> Properties { get; } = [];

        //public Collection<ZSObject> NestedTypes { get; } = new();

        //public Collection<Event> Events { get; }

        public Collection<Constructor> Constructors { get; } = [];
    }
}
