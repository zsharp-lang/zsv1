using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public interface IParameter 
        : CompilerObject
        , ITyped
    {
        public string Name { get; }

        public CompilerObject? Default { get; }

        public CompilerObjectResult MatchArgument(Compiler.Compiler compiler, CompilerObject argument);
    }
}
