using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal interface IGetIndex<T>
    {
        public CTObject Index(T @object, Argument[] index);
    }
}
