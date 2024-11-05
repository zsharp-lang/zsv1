namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        public ZSBoolean True { get; private set; } = null!;

        public ZSBoolean False { get; private set; } = null!;

        private void InitializeSingletonValues()
        {
            True = new ZSBoolean(true, TypeSystem.Boolean);
            False = new ZSBoolean(false, TypeSystem.Boolean);
        }
    }
}
