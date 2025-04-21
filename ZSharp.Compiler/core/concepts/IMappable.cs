namespace ZSharp.Compiler
{
    public interface IMappable
        : CompilerObject
    {
        public CompilerObject Map(Func<CompilerObject, CompilerObject> func);
    }
}
