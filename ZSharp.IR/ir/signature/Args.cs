using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Args
    {
        private ArgsCollection? _parameters;

        public Collection<Parameter> Parameters
        {
            get
            {
                if (_parameters is not null)
                    return _parameters;

                Interlocked.CompareExchange(ref _parameters, new(Signature), null);
                return _parameters;
            }
        }

        public Parameter? Var { get; set; }

        public int Length => Parameters.Count;

        public int TotalLength => Length + (Var is not null ? 1 : 0);

        public Signature Signature { get; }

        internal Args(Signature signature)
        {
            Signature = signature;
        }
    }
}
