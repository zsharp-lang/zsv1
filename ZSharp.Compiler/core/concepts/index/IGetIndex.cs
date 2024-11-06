namespace ZSharp.Compiler
{
    internal interface IGetIndex<T>
    {
        public CGObject Index(T @object, Argument[] index);
    }
}
