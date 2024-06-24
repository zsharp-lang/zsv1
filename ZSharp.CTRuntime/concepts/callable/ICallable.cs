namespace ZSharp.CTRuntime
{
    public interface ICallable<T>
    {
        public T Call(ZSCompiler compiler, T callable, Argument<T>[] arguments);
    }
}
