namespace ZSharp.RAST
{
    public class RCall : RExpression
    {
        public RExpression Callee { get; set; }

        public RArgument[] Arguments { get; set; }

        public RCall(RExpression callable, RArgument[] arguments)
        {
            Callee = callable;
            Arguments = arguments;
        }
    }
}
