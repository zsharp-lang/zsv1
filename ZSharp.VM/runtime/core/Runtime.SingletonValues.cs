namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        public ZSObject True { get; private set; } = null!;

        public ZSObject False { get; private set; } = null!;

        private void InitializeSingletonValues()
        {
            True = new ZSValue(TypeSystem.Boolean);
            False = new ZSValue(TypeSystem.Boolean);
        }
    }
}
