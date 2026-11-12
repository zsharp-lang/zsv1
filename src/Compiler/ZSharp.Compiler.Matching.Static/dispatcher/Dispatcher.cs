namespace ZSharp.Compiler.MatchingDispatchers.Static
{
    /// <summary>
    /// Matches through what the COs say of themselves: a parameter takes its value from
    /// the arguments, and a value matches a pattern as it says, or else as what describes
    /// it at runtime says. Each asks the dispatchers applied before it first.
    /// </summary>
    public sealed class Dispatcher(Matching matching, TS ts)
    {
        private readonly Matching matching = matching;
        private readonly TS ts = ts;
        private Matching @base = null!;

        public void Apply()
        {
            @base = matching.Clone();

            matching.Pattern = Pattern;
            matching.Parameter = Parameter;
        }

        public IResult Pattern(CompilerObject value, CompilerObject pattern)
        {
            var result = @base.Match(value, pattern);

            if (result.IsUnsupported() && value.Is<ICTMatch>(out var match))
                result = match.Match(pattern);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, value, out var descriptor) && descriptor.Is<IRTMatch>(out var described))
                result = described.Match(value, pattern);

            return result;
        }

        public IResult Parameter(CompilerObject parameter, ArgumentStream arguments)
        {
            var result = @base.Match(parameter, arguments);

            if (result.IsUnsupported() && parameter.Is<ICTParameter>(out var match))
                result = match.Match(arguments);

            return result;
        }
    }
}
