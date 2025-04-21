namespace ZSharp.Objects
{
    public interface IParameter : CompilerObject
    {
        public string Name { get; }

        public CompilerObject? Default { get; }

        public CompilerObject MatchArgument(Compiler.Compiler compiler, CompilerObject argument);
    }
}
