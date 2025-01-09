namespace ZSharp.AST
{
    public sealed class Module(ModuleTokens tokens) : Expression(tokens)
    {
        public new ModuleTokens TokenInfo
        {
            get => As<ModuleTokens>();
            init => base.TokenInfo = value;
        }

        public List<Statement> Body { get; set; } = [];

        public required string Name { get; set; }
    }
}
