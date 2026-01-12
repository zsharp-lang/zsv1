using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Class(string? name) 
        : TypeDefinition
        , ICustomMetadataProvider
    {
        private Collection<CustomMetadata>? _customMetadata;
        private Collection<GenericParameter>? _genericParameters;

        private Collection<Constructor>? _constructors;
        private FieldCollection? _fields;
        private Collection<Method>? _methods;

        public string? Name { get; set; } = name;

        public ClassAttributes Attributes { get; set; } = ClassAttributes.None;

        public TypeReference<Class>? Base { get; set; }

        public Class(string? name, TypeReference<Class>? @base)
            : this(name)
        {
            Base = @base;
        }

        public Collection<CustomMetadata> CustomMetadata
        {
            get
            {
                if (_customMetadata is not null)
                    return _customMetadata;

                Interlocked.CompareExchange(ref _customMetadata, [], null);
                return _customMetadata;
            }
        }

        public bool HasCustomMetadata => !_customMetadata.IsNullOrEmpty();

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

        public Collection<Constructor> Constructors
        {
            get
            {
                if (_constructors is not null)
                    return _constructors;

                Interlocked.CompareExchange(ref _constructors, [], null);

                return _constructors;
            }
        }

        public bool HasConstructors => !_constructors.IsNullOrEmpty();

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

        public Collection<Method> Methods
        {
            get
            {
                if (_methods is not null)
                    return _methods;

                Interlocked.CompareExchange(ref _methods, [], null);

                return _methods;
            }
        }

        public bool HasMethods => !_methods.IsNullOrEmpty();

        public Collection<Property> Properties { get; } = [];

        public Collection<TypeDefinition> NestedTypes { get; } = [];

        //public Collection<Event> Events { get; }
    }
}
