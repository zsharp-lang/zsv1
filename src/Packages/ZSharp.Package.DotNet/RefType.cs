namespace Package.DotNet
{
    public sealed class RefType(ZSharp.HIR.Type target)
        : TypeReference
    {
        public ZSharp.HIR.Type Target { get; set; } = target;
    }
}
