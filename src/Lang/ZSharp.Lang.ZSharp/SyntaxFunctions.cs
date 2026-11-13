namespace ZSharp.Lang
{
    /// <summary>
    /// The functions a piece of syntax stands for, by the syntax, as an operator literal
    /// names them: <c>(+)</c>. It is a value, so what says a definition is one of them, as
    /// <c>@operator((+))</c>, keeps it with the definition, as its syntax, and what holds
    /// the definition adds the definition to the language's.
    /// </summary>
    public sealed class SyntaxFunctions(string syntax)
    {
        public string Syntax { get; set; } = syntax;
    }
}
