namespace ZSharp.Objects
{
    public interface IKeywordVarParameter : CompilerObject
    {
        public CompilerObject MatchArguments(Compiler.Compiler compiler, Dictionary<string, CompilerObject> argument);
    }
}
