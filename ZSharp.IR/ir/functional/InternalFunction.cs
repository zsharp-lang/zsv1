namespace ZSharp.IR
{
    public sealed class InternalFunction(IType returnType) : IRObject
    {
        public string? Name { get; set; }

        public Signature Signature { get; } = new(returnType);
    }
}
