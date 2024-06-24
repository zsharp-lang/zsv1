using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    public sealed class DesugaringContext
    {
        private readonly Dictionary<string, RExpression> operators = new();

        public RExpression ImportFunction { get; set; } = new RId("import");

        public RExpression GetBinaryOperator(string @operator)
        {
            @operator = '_' + @operator + '_';
            if (!operators.TryGetValue(@operator, out RExpression? result))
            {
                operators.Add(@operator, result = new RId(@operator));
            }
            return result;
        }
    }
}
