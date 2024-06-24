namespace ZSharp.RAST
{
    public sealed class RIfStatement : RStatement
    {
        public RExpression Condition { get; set; }

        public RStatement If { get; set; }

        public RStatement? Else { get; set; }

        public RIfStatement(RExpression condition, RStatement @if, RStatement? @else = null)
        {
            Condition = condition;
            If = @if;
            Else = @else;
        }
    }
}
