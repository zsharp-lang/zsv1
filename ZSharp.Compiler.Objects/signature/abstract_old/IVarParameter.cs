using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public interface IVarParameter 
        : CompilerObject
        , INamedObject
        , ITyped
    {
        public CompilerObject MatchArguments(Compiler.Compiler compiler, Collection<Compiler.Argument> arguments);
    }
}
