namespace ZSharp.Compiler
{
    internal interface IGetIndex<T>
    {
        public CompilerObject Index(T @object, Argument[] index);
    }
}
