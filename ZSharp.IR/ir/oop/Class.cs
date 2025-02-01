using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Class(string? name) : OOPType
    {
        private Collection<GenericParameter>? _genericParameters;

        private FieldCollection? _fields;

        public string? Name { get; set; } = name;

        public ClassAttributes Attributes { get; set; } = ClassAttributes.None;

        public ConstructedClass? Base { get; set; }

        public Class(string? name, ConstructedClass? @base)
            : this(name)
        {
            Base = @base;
        }

        public Collection<InterfaceImplementation> InterfacesImplementations { get; } = [];

        public Collection<GenericParameter> GenericParameters
        {
            get
            {
                if (_genericParameters is not null)
                    return _genericParameters;

                Interlocked.CompareExchange(ref _genericParameters, [], null);
                return _genericParameters;
            }
        }

        public bool HasGenericParameters => !_genericParameters.IsNullOrEmpty();

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
