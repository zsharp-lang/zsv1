using ZSharp.IR.VM;

namespace ZSharp.IR
{
    public class Parameter(string name, IType type) : IRObject
    {
        private Signature? _signature;

        public string Name { get; } = name;

        public IType Type { get; set; } = type;

        public Instruction[]? Initializer { get; set; }

        public Signature? Signature
        {
            get => _signature;
            set => Module = (_signature = value)?.Module;
        }

        public int Index { get; internal set; }
    }
}
