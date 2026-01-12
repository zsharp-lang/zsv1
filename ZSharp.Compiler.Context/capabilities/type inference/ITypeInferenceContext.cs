namespace ZSharp.Compiler
{
    public interface ITypeInferenceContext
    {
        protected sealed T OnCreate<T>(T inferrer)
            where T : TypeInferrer
        {
            inferrer.Context = this;
            return inferrer;
        }
    }
}
