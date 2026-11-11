namespace ZSharp.LIR
{
    public sealed class Reference(Type target) : Type
    {
        public Type Target { get; set; } = target;
    }
}
