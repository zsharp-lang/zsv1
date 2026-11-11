namespace ZSharp.AST
{
    public sealed class DefinitionStatement : Statement
    {
        public required Expression Definition { get; init; }

        public override string ToString()
            => $"{Definition}";
    }
}
