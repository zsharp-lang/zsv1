namespace ZSharp.VM.Types
{
    internal sealed class Type : PrimitiveType
    {
        public Type(IR.Class @class) : base(@class, null!)
            => Type = this;
    }
}
