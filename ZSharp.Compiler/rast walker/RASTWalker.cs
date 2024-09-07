using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal partial class RExpressionWalker<T, V>(Func<V, IEnumerable<T>> walkFn)
        where V : RExpression
    {
        private readonly Func<V, IEnumerable<T>> walkFn = walkFn;

        public RExpressionWalker(Func<V, T?> walkFn)
            : this(expression =>
            {
                var result = walkFn(expression);
                return result is null ? [] : [result];
            })
        { }

        public IEnumerable<T> Walk(RExpression? expression)
            => expression switch
            {
                null => [],
                V value => Talk(value),
                RBinaryExpression binary => Walk(binary),
                RCall call => Walk(call),
                //RDefinition definition => Walk(definition),
                RFunction function => Walk(function),
                RId id => Walk(id),
                RLetDefinition let => Walk(let),
                RLiteral literal => Walk(literal),
                RModule module => Walk(module),
                ROOPDefinition oop => Walk(oop),
                ROperator @operator => Walk(@operator),
                RParameter parameter => Walk(parameter),
                RVarDefinition var => Walk(var),
                _ => throw new NotImplementedException()
            };

        public IEnumerable<T> Walk(RStatement? statement)
            => statement switch
            {
                null => [],
                RBlock block => block.Statements.SelectMany(Walk),
                RExpressionStatement expressionStatement => Walk(expressionStatement.Expression),
                RReturn @return => Walk(@return.Value),
                _ => throw new NotImplementedException(),
            };

        private IEnumerable<T> Talk(V expression)
            => walkFn(expression);
    }
}
