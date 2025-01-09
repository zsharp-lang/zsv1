namespace ZSharp.AST
{
    public sealed class Parameter(ParameterTokens tokens) : Node(tokens)
    {
        public new ParameterTokens TokenInfo
        {
            get => As<ParameterTokens>();
            init => base.TokenInfo = value;
        }

        public string? Alias { get; set; }

        public required string Name { get; set; }

        public Expression? Type { get; set; }

        public Expression? Initializer { get; set; }

        public override string ToString()
            => $"{Name}" +
            $"{(Type is null ? string.Empty : $": {Type}")}" +
            $"{(Initializer is null ? string.Empty : $" = {Initializer}")}";
    }
}
