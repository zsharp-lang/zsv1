namespace ZSharp.Lang
{
    /// <summary>
    /// Whether code can run while compiling: it refers to no <see cref="HIR.Variable"/>,
    /// nothing whose value only exists while code that runs later is running. This is the
    /// line the reifier draws between what it encodes and what it places as it is. A node
    /// this doesn't know is taken to need what runs later, to be safe.
    /// </summary>
    public static class CompileTime
    {
        public static bool Is(HIRExpression expression)
            => expression switch
            {
                HIR.Code.Get get => get.Instance is not HIR.Variable,
                HIR.Code.StringLiteral or HIR.Code.BooleanLiteral or HIR.Code.IntegerLiteral or HIR.Code.DecimalLiteral or HIR.Code.NullLiteral => true,
                HIR.Code.NamedMember member => Is(member.Target),
                HIR.Code.PositionalMember member => Is(member.Target),
                HIR.Code.Cast cast => Is(cast.Value) && Is(cast.Type),
                HIR.Code.Match match => Is(match.Value) && Is(match.Pattern),
                HIR.Code.Call call => Is(call.Callee) && call.Arguments.All(argument => Is(argument.Value)),
                HIR.Code.Index index => Is(index.Target) && index.Arguments.All(argument => Is(argument.Value)),
                _ => false
            };
    }
}
