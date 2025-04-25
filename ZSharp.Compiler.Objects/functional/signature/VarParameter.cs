namespace ZSharp.Objects
{
    public sealed class VarParameter(string name)
        : CompilerObject
        , IVarParameter
    {
        public string Name { get; set; } = name;

        public IR.Parameter? IR { get; set; }

        public Compiler.IType? Type { get; set; }

        CompilerObject IVarParameter.MatchArguments(Compiler.Compiler compiler, CompilerObject[] argument)
        {
            throw new NotImplementedException();
        }
    }
}
