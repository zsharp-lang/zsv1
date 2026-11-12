namespace ZSharp.Lang
{
    /// <summary>
    /// Whether code can run while compiling: it refers to no <see cref="HIR.Variable"/>,
    /// nothing whose value only exists while code that runs later is running. This is the
    /// line the reifier draws between what it encodes and what it places as it is.
    /// <para>
    /// Each kind of node says it through a handler, and a library that adds a kind registers
    /// its handler next to its loader. A node no handler knows is taken to need what runs
    /// later, to be safe. A pattern is known only if it binds nothing, as a value pattern
    /// doesn't.
    /// </para>
    /// </summary>
    public sealed class CompileTime
    {
        private readonly Dictionary<Type, Func<HIRExpression, bool>> expressions = [];
        private readonly Dictionary<Type, Func<HIR.Code.Pattern, bool>> patterns = [];

        public CompileTime()
        {
            Register<HIR.Code.Get>(get => get.Instance is not HIR.Variable);
            Register<HIR.Code.StringLiteral>(_ => true);
            Register<HIR.Code.BooleanLiteral>(_ => true);
            Register<HIR.Code.IntegerLiteral>(_ => true);
            Register<HIR.Code.DecimalLiteral>(_ => true);
            Register<HIR.Code.NullLiteral>(_ => true);
            Register<HIR.Code.NamedMember>(member => Is(member.Target));
            Register<HIR.Code.PositionalMember>(member => Is(member.Target));
            Register<HIR.Code.Cast>(cast => Is(cast.Value) && Is(cast.Type));
            Register<HIR.Code.Match>(match => Is(match.Value) && Is(match.Pattern));
            Register<HIR.Code.ArrayLiteral>(array => array.Items.All(Is));
            Register<HIR.Code.Call>(call => Is(call.Callee) && call.Arguments.All(argument => Is(argument.Value)));
            Register<HIR.Code.Index>(index => Is(index.Target) && index.Arguments.All(argument => Is(argument.Value)));

            RegisterPattern<HIR.Code.ValuePattern>(pattern => Is(pattern.Value));
        }

        /// <summary>
        /// Says whether a kind of expression can run while compiling, replacing what was said
        /// of it.
        /// </summary>
        public void Register<T>(Func<T, bool> known)
            where T : HIRExpression
            => expressions[typeof(T)] = expression => known((T)expression);

        /// <summary>
        /// Says whether a kind of pattern can be matched while compiling.
        /// </summary>
        public void RegisterPattern<T>(Func<T, bool> known)
            where T : HIR.Code.Pattern
            => patterns[typeof(T)] = pattern => known((T)pattern);

        public bool Is(HIRExpression expression)
            => expressions.TryGetValue(expression.GetType(), out var known) && known(expression);

        public bool Is(HIR.Code.Pattern pattern)
            => patterns.TryGetValue(pattern.GetType(), out var known) && known(pattern);
    }
}
