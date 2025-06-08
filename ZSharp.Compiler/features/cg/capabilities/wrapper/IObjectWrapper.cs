namespace ZSharp.Compiler
{
    public interface IObjectWrapper<T>
        : CompilerObject
    {
        public T MapWrapped(Compiler compiler, Func<CompilerObject, T> fn);
    }
}
