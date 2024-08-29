namespace ZSharp.AST
{
    public class Function(FunctionTokens tokens) : Expression(tokens)
    {
        public new FunctionTokens TokenInfo
        {
            get => As<FunctionTokens>();
            init => base.TokenInfo = value;
        }

        public string? Name { get; init; }

        public required Signature Signature { get; set; }

        public Expression? ReturnType { get; set; }

        public Statement? Body { get; set; }

        public override string ToString()
            => $"fun{(Name is null ? string.Empty : $" {Name}")}" +
            $"({Signature}){(ReturnType is null ? string.Empty : $": {ReturnType}")}" +
            $"{(Body is null ? ";" : " {}")}";
    }
}
