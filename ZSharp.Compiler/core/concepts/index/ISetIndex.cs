namespace ZSharp.Compiler
{
    internal interface ISetIndex<T>
    {
        public CompilerObject Index(T @object, Argument[] index, CompilerObject value);
    }
}
