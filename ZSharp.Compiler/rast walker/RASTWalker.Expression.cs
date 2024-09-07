using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal partial class RExpressionWalker<T, V>
    {
        public IEnumerable<T> Walk(RBinaryExpression binary)
            => [
                ..Walk(binary.Left),
                ..Walk(binary.Right),
                ..Walk(new ROperator(binary.Operator))
            ];

        public IEnumerable<T> Walk(RCall call)
            => [
                ..Walk(call.Callee),
                ..call.Arguments.SelectMany(arg => Walk(arg.Value))
            ];

        public IEnumerable<T> Walk(RId _)
            => [];

        public IEnumerable<T> Walk(RLiteral _)
            => [];

        public IEnumerable<T> Walk(ROperator _)
            => [];
    }
}
