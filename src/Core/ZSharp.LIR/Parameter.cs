namespace ZSharp.LIR
{
    public sealed class Parameter(Type type)
        : Definition
        , Variable
    {
        public Type Type { get; set; } = type;
    }
}
