namespace ZSharp.VM
{
    internal interface ICallable
    {
        public int ArgumentCount { get; }

        public Instruction[] Code { get; }

        public int LocalCount { get; }

        public int StackSize { get; }
    }
}
