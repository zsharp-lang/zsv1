namespace ZSharp.Compiler.OverloadingDispatchers.Static
{
    /// <summary>
    /// Chooses the most specific candidate an operation applies to, as the candidates
    /// answer through <see cref="ISpecific"/>.
    /// </summary>
    public sealed class Dispatcher(Overloading overloading)
    {
        private readonly Overloading overloading = overloading;
        private Overloading @base = null!;

        public void Apply()
        {
            @base = overloading.Clone();

            overloading.Apply = Apply;
            overloading.IsMoreSpecific = IsMoreSpecific;
            overloading.Group = Group;
        }

        /// <summary>
        /// Candidates stand for one overload group, which applies each operation to every
        /// one of them and chooses whose result is the result.
        /// </summary>
        public IResult Group(CompilerObject[] candidates)
            => Result.Ok(OverloadGroup.Of(candidates));

        public IResult Apply(CompilerObject[] candidates, Func<CompilerObject, IResult> operation)
        {
            var results = candidates.Select(operation).ToArray();
            var applied = Enumerable.Range(0, candidates.Length).Where(index => results[index].IsOk).ToArray();

            if (applied is [var only])
                return results[only];

            if (applied is [])
                return Failed(results);

            var best = applied
                .Where(index => applied.All(other =>
                    other == index || overloading.IsMoreSpecific(candidates[index], candidates[other])
                ))
                .ToArray();

            return best is [var chosen]
                ? results[chosen]
                : Result.Error($"{applied.Length} overloads apply, and none of them is more specific than the others.");
        }

        /// <summary>
        /// A candidate is more specific than another if it is at least as specific and the
        /// other one isn't.
        /// </summary>
        public bool IsMoreSpecific(CompilerObject candidate, CompilerObject other)
            => @base.IsMoreSpecific(candidate, other)
            || candidate.Is<ISpecific>(out var specific) && specific.IsAsSpecificAs(other)
            && !(other.Is<ISpecific>(out var then) && then.IsAsSpecificAs(candidate));

        /// <summary>
        /// Why nothing applies. A single candidate's error is the error, and the operation
        /// is unsupported only if every candidate says so.
        /// </summary>
        private static IResult Failed(IResult[] results)
        {
            if (results is [var single])
                return single;

            if (results is [])
                return Result.Error("There is nothing to choose from.");

            var errors = results.Select(result => result.UnwrapError()).ToArray();

            if (errors.All(error => error.IsUnsupported()))
                return Result.Error(errors[0]);

            return Result.Error(
                $"None of the {results.Length} overloads applies:"
                + string.Concat(errors.Select(error => $"\n  {error}"))
            );
        }
    }
}
