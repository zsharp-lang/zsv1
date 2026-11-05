namespace ZSharp.LIR
{
    public sealed class Parameter(Type type) : Definition
    {
        public Type Type { get; set; } = type;
    }
}
