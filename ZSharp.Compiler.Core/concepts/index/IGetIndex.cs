namespace ZSharp.CTRuntime
{
    internal interface IGetIndex<T>
    {
        public Code Index(T @object, Argument[] index);
    }
}
