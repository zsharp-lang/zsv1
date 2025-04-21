using CommonZ.Utils;

namespace ZSharp.Objects
{
    public interface IVarParameter : CompilerObject
    {
        public CompilerObject MatchArguments(Compiler.Compiler compiler, CompilerObject[] argument);
    }
}
