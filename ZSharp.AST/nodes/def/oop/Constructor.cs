namespace ZSharp.AST
{
    public sealed class Constructor(ConstructorTokens tokens) : Statement(tokens)
    {
        public new ConstructorTokens TokenInfo
        {
            get => As<ConstructorTokens>();
            init => base.TokenInfo = value;
        }

        public string? Name { get; set; }

        public required Signature Signature { get; set; }

        public List<Expression>? Initializers { get; set; }

        public Statement? Body { get; set; }

        public override string ToString()
            => $"new{(Name is null ? string.Empty : $" {Name}")}" +
            $"({Signature}){(Initializers is null ? string.Empty : $": {string.Join(",\n", Initializers)}")}" +
            $"{(Body is null ? ";" : " {}")}";
    }
}
