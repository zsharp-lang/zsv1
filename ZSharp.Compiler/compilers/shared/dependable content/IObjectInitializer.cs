namespace ZSharp.Compiler
{
    internal interface IObjectInitializer<T>
    {
        public void Initialize(T obj);
    }
}
