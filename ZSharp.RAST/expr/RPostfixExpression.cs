namespace ZSharp.RAST
{
    public class RPostfixExpression : RExpression
    {
        public string Operator { get; set; }

        public RExpression Left { get; set; }

        public RPostfixExpression(string op, RExpression left)
        {
            Operator = op;
            Left = left;
        }
    }
}
