using ZSharp.IR.VM;

namespace ZSharp.IR
{
    public class Parameter(string name, IType type) 
        : IRDefinition
        , IModuleMember
    {
        private Signature? _signature;

        public Module? Module => _signature?.Module;

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
