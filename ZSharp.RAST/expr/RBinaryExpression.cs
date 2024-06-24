namespace ZSharp.RAST
{
    public class RBinaryExpression : RExpression
    {
        public RExpression Left { get; set; }

        public RExpression Right { get; set; }

        public string Operator { get; set; }

        public RBinaryExpression(RExpression left, RExpression right, string op)
        {
            Left = left;
            Right = right;
            Operator = op;
        }
    }
}
