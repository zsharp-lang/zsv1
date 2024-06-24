namespace ZSharp.RAST
{
    public enum ExpressionPatternType
    {
        ByValue,
        ByType,
    }

    public sealed class RExpressionPattern : RPattern
    {
        public ExpressionPatternType Type { get; set; }

        public RExpression Value { get; set; }

        public RExpressionPattern(RExpression value, ExpressionPatternType type)
        {
            Type = type;
            Value = value;
        }

        public static RExpressionPattern ByValue(RExpression value)
        {
            return new RExpressionPattern(value, ExpressionPatternType.ByValue);
        }

        public static RExpressionPattern ByType(RExpression value)
        {
            return new RExpressionPattern(value, ExpressionPatternType.ByType);
        }
    }
}
