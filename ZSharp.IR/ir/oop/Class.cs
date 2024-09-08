using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Class(string? name) : OOPType
    {
        private FieldCollection? _fields;

        public string? Name { get; set; } = name;

        public ClassAttributes Attributes { get; set; } = ClassAttributes.None;

        public Class? Base { get; set; }

        public Class(string? name, Class? @base)
            : this(name)
        {
            Base = @base;
        }

        public Collection<Interface> Interfaces { get; } = [];

        public Collection<Field> Fields
        {
            get
            {
                if (_fields is not null)
                    return _fields;

                Interlocked.CompareExchange(ref _fields, new(this), null);
                return _fields;
            }
        }

        public bool HasFields => !_fields.IsNullOrEmpty();

        public Collection<Method> Methods { get; } = [];

        public Collection<Property> Properties { get; } = [];

        //public Collection<ZSObject> NestedTypes { get; } = new();

        //public Collection<Event> Events { get; }

        public Collection<Constructor> Constructors { get; } = [];
    }
}
