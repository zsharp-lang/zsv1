namespace ZSharp.RAST
{
    public sealed class RIfExpression : RExpression
    {
        public RExpression Condition { get; set; }

        public RExpression If { get; set; }

        public RExpression Else { get; set; }

        public RIfExpression(RExpression condition, RExpression @if, RExpression @else)
        {
            Condition = condition;
            If = @if;
            Else = @else;
        }
    }
}
