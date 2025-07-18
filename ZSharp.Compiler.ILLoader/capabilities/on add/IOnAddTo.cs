namespace ZSharp.Objects
{
    public interface IOnAddTo<T>
        where T : class, CompilerObject
    {
        public OnAddResult OnAddTo(T @object);
    }
}
