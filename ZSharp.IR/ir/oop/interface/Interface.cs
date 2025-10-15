using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Interface(string? name) : TypeDefinition
    {
        private Collection<GenericParameter>? _genericParameters;
        private Collection<TypeReference<Interface>>? _bases;
        private Collection<Method>? _methods;

        public string? Name { get; set; } = name;

        public InterfaceAttributes Attributes { get; set; } = InterfaceAttributes.None;

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

        public Collection<TypeReference<Interface>> Bases
        {
            get
            {
                if (_bases is not null)
                    return _bases;

                Interlocked.CompareExchange(ref _bases, [], null);
                return _bases;
            }
        }

        public bool HasBases => !_bases.IsNullOrEmpty();

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

        //public Collection<Event> Events { get; }

        //public Collection<ZSObject> NestedTypes { get; }
    }
}
