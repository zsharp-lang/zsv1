namespace ZSharp.LIR
{
    public sealed class Local(Type type)
        : Definition
        , Variable
    {
        public Type Type { get; set; } = type;
    }
}
