namespace ZSharp.VM.Types
{
    internal sealed class Type : PrimitiveType
    {
        public Type() : base("Type", null!)
            => Type = this;
    }
}
