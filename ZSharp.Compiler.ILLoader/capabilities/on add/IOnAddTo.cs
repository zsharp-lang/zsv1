namespace ZSharp.Objects
{
    public interface IOnAddTo<T>
        where T : class, Compiler.CompilerObject
    {
        public OnAddResult OnAddTo(T @object);
    }
}
