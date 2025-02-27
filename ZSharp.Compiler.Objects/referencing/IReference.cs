namespace ZSharp.Objects
{
    public interface IReference
    {
        public CompilerObject Origin { get; }

        public ReferenceContext? Context { get; set; }
    }
}
