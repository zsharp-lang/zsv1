namespace ZSharp.HIR.Code
{
    public sealed class Code
        : Annotated
    {
        public Block Block { get; } = new();

        public List<object> Annotations { get; } = [];
    }
}
