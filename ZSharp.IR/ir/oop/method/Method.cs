namespace ZSharp.IR
{
    public sealed class Method 
        : IRObject
        , ICallable
    {
        private Signature _signature;
        private VM.MethodBody? _body;

        public string? Name { get; set; }

        public MethodAttributes Attributes { get; set; } = MethodAttributes.None;

        public IType ReturnType
        {
            get => _signature.ReturnType;
            set => _signature.ReturnType = value;
        }

        public Signature Signature
        {
            get => _signature;
            set
            {
                if (value.Owner is not null)
                    throw new InvalidOperationException();
                _signature.Owner = null;
                (_signature = value).Owner = this;
            }
        }

        public VM.MethodBody Body
        {
            get
            {
                if (_body is not null)
                    return _body;

                Interlocked.CompareExchange(ref _body, new(this), null);
                return _body;
            }
        }

        public bool HasBody => _body is not null;

        public OOPType? Owner { get; set; }

        public override Module? Module => Owner?.Module;

        public bool IsClass
        {
            get => (Attributes & MethodAttributes.ClassMethod) == MethodAttributes.ClassMethod;
            set => Attributes = value
                ? Attributes | MethodAttributes.ClassMethod
                : Attributes & ~MethodAttributes.ClassMethod;
        }

        public bool IsInstance
        {
            get => (Attributes & MethodAttributes.InstanceMethod) == MethodAttributes.InstanceMethod;
            set => Attributes = value
                ? Attributes | MethodAttributes.InstanceMethod
                : Attributes & ~MethodAttributes.InstanceMethod;
        }

        public bool IsStatic
        {
            get => (Attributes & MethodAttributes.StaticMethod) == MethodAttributes.StaticMethod;
            set => Attributes = value
                ? Attributes | MethodAttributes.StaticMethod
                : Attributes & ~MethodAttributes.StaticMethod;
        }

        public bool IsVirtual
        {
            get => (Attributes & MethodAttributes.VirtualMethod) == MethodAttributes.VirtualMethod;
            set => Attributes = value
                ? Attributes | MethodAttributes.VirtualMethod
                : Attributes & ~MethodAttributes.VirtualMethod;
        }

        public Method(IType returnType)
        {
            _signature = new(returnType) { Owner = this };
        }

        public Method(Signature signature)
            : this((null as IType)!)
        {
            Signature = signature;
        }
    }
}
