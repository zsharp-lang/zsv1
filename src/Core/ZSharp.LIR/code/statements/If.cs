namespace ZSharp.LIR
{
    public sealed class If(Expression condition, Statement then, Statement? @else = null)
        : Statement
    {
        public Expression Condition { get; set; } = condition;

        public Statement Then { get; set; } = then;

        public Statement? Else { get; set; } = @else;
    }
}
