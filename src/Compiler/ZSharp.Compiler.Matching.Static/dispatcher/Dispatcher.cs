namespace ZSharp.Compiler.MatchingDispatchers.Static
{
    /// <summary>
    /// Matches through what the COs say of themselves: a parameter takes its value from
    /// the arguments, and a value matches a pattern as the pattern says, or else as the
    /// value says, or else as what describes it at runtime says. Each asks the dispatchers applied before it first.
    /// </summary>
    public sealed class Dispatcher(Matching matching, TS ts)
    {
        private readonly Matching matching = matching;
        private readonly TS ts = ts;
        private Matching @base = null!;

        public void Apply()
        {
            @base = matching.Clone();

            matching.MatchPattern = Match;
            matching.BindParameter = Bind;
        }

        public IResult Match(CompilerObject value, CompilerObject pattern)
        {
            var result = @base.Match(value, pattern);

            // A pattern that knows how it matches says so first, as one that binds does.
            if (result.IsUnsupported() && pattern.Is<ICTPattern>(out var known))
                result = known.Match(value);

            if (result.IsUnsupported() && value.Is<ICTMatch>(out var match))
                result = match.Match(pattern);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, value, out var descriptor) && descriptor.Is<IRTMatch>(out var described))
                result = described.Match(value, pattern);

            return result;
        }

        public IResult Bind(CompilerObject parameter, ArgumentStream arguments)
        {
            var result = @base.Bind(parameter, arguments);

            if (result.IsUnsupported() && parameter.Is<ICTBind>(out var bound))
                result = bound.Bind(arguments);


            return result;
        }
    }
}
