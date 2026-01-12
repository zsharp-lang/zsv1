namespace ZSharp.AST
{
    public sealed class Signature : Node
    {
        public List<Parameter>? Args { get; set; }

        public Parameter? VarArgs { get; set; }

        public List<Parameter>? KwArgs { get; set; }

        public Parameter? VarKwArgs { get; set; }

        public override string ToString()
            => string.Join(", ", 
                new List<string?>([
                    Args is null ? null : string.Join(", ", Args),
                    VarArgs is null ? null : $"*{VarArgs}",
                    KwArgs is null ? null : "{" + string.Join(", ", KwArgs) + (VarKwArgs is null ? "}" : string.Empty),
                    VarKwArgs is null ? null : (KwArgs is null ? "{" : string.Empty) + $"*{VarKwArgs}}}",
                ]).Where(item => item is not null)
            );
    }
}
