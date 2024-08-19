namespace ZSharp.CTRuntime
{
    internal interface ISetIndex<T>
    {
        public Code Index(T @object, Argument[] index, Code value);
    }
}
