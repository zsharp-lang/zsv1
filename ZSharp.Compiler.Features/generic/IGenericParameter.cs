namespace ZSharp.Compiler.Features.Generic
{
    public interface IGenericParameter : CompilerObject
    {
        public string Name { get; }

        public CompilerObject Owner { get; }
    }
}
