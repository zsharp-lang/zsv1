namespace ZSharp.Objects
{
    public interface IGenericParameter : CompilerObject
    {
        public string Name { get; }

        public CompilerObject Owner { get; }
    }
}
