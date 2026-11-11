namespace ZSharp.LIR
{
    public sealed class Global(Type type) : ModuleMember
    {
        public Type Type { get; set; } = type;
    }
}
