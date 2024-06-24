namespace ZSharp.IR
{
    public class Function : IRObject
    {
        private Signature _signature;
        private VM.FunctionBody? _body;

        public string? Name { get; set; }

        public FunctionAttributes Attributes { get; set; } = FunctionAttributes.None;

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

        public VM.FunctionBody Body
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

        //public Collection<Clause> Clauses { get; }

        public Function(IType returnType)
        {
            _signature = new(returnType) { Owner = this };
        }

        public Function(Signature signature)
            : this((null as IType)!)
        {
            Signature = signature;
        }
    }
}
