namespace ZSharp.IR
{
    public abstract class ModuleMember : IRObject
    {
        private Module? _module;

        public override Module? Module => _module;

        public Module? Owner
        {
            get => _module;
            set => _module = value;
        }
    }
}
