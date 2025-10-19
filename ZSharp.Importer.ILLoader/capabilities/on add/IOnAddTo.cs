namespace ZSharp.Objects
{
    public interface IOnAddTo<in T>
        where T : class, CompilerObject
    {
        public OnAddResult OnAddTo(T @object);
    }
}
