using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public interface IStatementCompiler
    {
        public CGObject? Compile(RStatement statement);
    }
}
