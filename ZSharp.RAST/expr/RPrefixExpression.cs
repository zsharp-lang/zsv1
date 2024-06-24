namespace ZSharp.RAST
{
    public class RPrefixExpression : RExpression
    {
        public string Operator { get; set; }

        public RExpression Right { get; set; }

        public RPrefixExpression(string op, RExpression right)
        {
            Operator = op;
            Right = right;
        }
    }
}
