namespace ZSharp.IR
{
    public sealed class Method(IType returnType) : IRObject
    {
        private readonly Function _function = new(returnType);

        public string? Name { get; set; }

        public MethodAttributes Attributes { get; set; } = MethodAttributes.None;

        public IType ReturnType
        {
            get => _function.ReturnType;
            set => _function.ReturnType = value;
        }

        public Signature Signature
        {
            get => _function.Signature;
            set
            {
                if (value.Owner is not null)
                    throw new InvalidOperationException();
                _function.Signature.Owner = null;
                (_function.Signature = value).Owner = this;
            }
        }

        public VM.FunctionBody Body => _function.Body;

        public bool HasBody => _function.HasBody;

        public OOPType? Owner { get; internal set; }

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

        public Method(Signature signature)
            : this((null as IType)!)
        {
            Signature = signature;
        }
    }
}
