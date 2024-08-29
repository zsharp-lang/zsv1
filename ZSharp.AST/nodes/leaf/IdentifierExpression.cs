namespace ZSharp.AST
{
    public class IdentifierExpression(string name) : Expression
    {
        public string Name { get; set; } = name;
    }
}
