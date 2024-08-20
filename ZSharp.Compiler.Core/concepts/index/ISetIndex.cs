using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal interface ISetIndex<T>
    {
        public CTObject Index(T @object, Argument[] index, CTObject value);
    }
}
