namespace Package.DotNet
{
    public sealed class ArrayType(ZSharp.HIR.Type element)
        : TypeReference
    {
        public ZSharp.HIR.Type Element { get; set; } = element;
    }
}
