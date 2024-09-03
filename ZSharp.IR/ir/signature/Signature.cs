namespace ZSharp.IR
{
    public sealed class Signature(IType returnType) : IRObject
    {
        private IRObject? _owner;

        private Args? _args;
        private KwArgs? _kwArgs;

        public override Module? Module => _owner?.Module;

        public Args Args
        {
            get
            {
                if (_args is not null)
                    return _args;

                Interlocked.CompareExchange(ref _args, new(this), null);
                return _args;
            }
        }

        public KwArgs KwArgs
        {
            get
            {
                if (_kwArgs is not null)
                    return _kwArgs;

                Interlocked.CompareExchange(ref _kwArgs, new(this), null);
                return _kwArgs;
            }
        }

        public bool IsVarArgs => _args?.Var is not null;

        public bool IsVarKwArgs => _kwArgs?.Var is not null;

        public bool HasArgs => _args is not null && _args.Parameters.Count > 0;

        public bool HasKwArgs => _kwArgs is not null && _kwArgs.Parameters.Count > 0;

        public int NumArgs => _args?.Length ?? 0;

        public int NumKwArgs => _kwArgs?.Length ?? 0;

        public int TotalNumArgs => NumArgs + (IsVarArgs ? 1 : 0);

        public int TotalNumKwArgs => NumKwArgs + (IsVarKwArgs ? 1 : 0);

        public int Length => TotalNumArgs + TotalNumKwArgs;

        public IType ReturnType { get; set; } = returnType;

        public IRObject? Owner
        {
            get => _owner;
            set => _owner = value;
        }

        public IEnumerable<Parameter> GetParameters(bool includeVarArgs = true, bool includeVarKwArgs = true)
        {
            if (_args is not null)
                foreach (var param in Args.Parameters)
                    yield return param;
            if (includeVarArgs && IsVarArgs)
                yield return Args.Var!;
            if (_kwArgs is not null)
                foreach (var param in KwArgs.Parameters)
                    yield return param;
            if (includeVarKwArgs && IsVarKwArgs)
                yield return KwArgs.Var!;
        }
    }
}
