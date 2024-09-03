using ZSharp.IR.VM;

namespace ZSharp.IR
{
    public class Parameter(string name, IType type) : IRObject
    {
        private Signature? _signature;

        public override Module? Module => _signature?.Module;

        public string Name { get; } = name;

        public IType Type { get; set; } = type;

        public Instruction[]? Initializer { get; set; }

        public Signature? Signature
        {
            get => _signature;
            set => _signature = value;
        }

        public int Index { get; internal set; }
    }
}
