namespace ZSharp.Objects
{
    public interface IParameter : CompilerObject
    {
        public string Name { get; }

        public CompilerObject? Default { get; }

        public CompilerObjectResult MatchArgument(Compiler.Compiler compiler, CompilerObject argument);
    }
}
