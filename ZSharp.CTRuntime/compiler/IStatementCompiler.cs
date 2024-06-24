using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal interface IStatementCompiler<T>
    {
        public IBinding<T> Compile(RStatement statement);
    }
}
