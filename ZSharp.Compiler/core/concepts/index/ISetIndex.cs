namespace ZSharp.Compiler
{
    internal interface ISetIndex<T>
    {
        public CGObject Index(T @object, Argument[] index, CGObject value);
    }
}
