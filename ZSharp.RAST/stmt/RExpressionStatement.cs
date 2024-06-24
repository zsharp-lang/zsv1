namespace ZSharp.RAST
{
    public class RExpressionStatement : RStatement
    {
        public RExpression Expression { get; }

        public RExpressionStatement(RExpression expression)
        {
            Expression = expression;
        }
    }
}
