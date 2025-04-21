namespace ZSharp.Objects
{
    public sealed class KeywordVarParameter(string name)
        : CompilerObject
        , IKeywordVarParameter
    {
        public string Name { get; set; } = name;

        public IR.Parameter? IR { get; set; }

        public CompilerObject? Type { get; set; }

        CompilerObject IKeywordVarParameter.MatchArguments(Compiler.Compiler compiler, Dictionary<string, CompilerObject> argument)
        {
            throw new NotImplementedException();
        }
    }
}
