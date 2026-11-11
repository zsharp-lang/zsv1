namespace ZSharp.LIR
{
    public sealed class Field(Type type) : Definition
    {
        public Type Type { get; set; } = type;
    }
}
