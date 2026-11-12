using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Compiler
{
    /// <summary>
    /// Several candidates one object stands for. Each operation is applied to every
    /// candidate, and overloading chooses whose result is the result.
    /// </summary>
    public sealed class OverloadGroup
        : CompilerObject
        , ICTCallable
        , ICTGetIndex
        , ICTImplicitCastTo
        , ICTCastTo
    {
        private readonly CompilerObject[] candidates;

        private OverloadGroup(CompilerObject[] candidates)
        {
            this.candidates = candidates;
        }

        /// <summary>
        /// What candidates stand for together. A single candidate stands for itself.
        /// </summary>
        internal static CompilerObject Of(IEnumerable<CompilerObject> candidates)
            => candidates.ToArray() is var all && all is [var single] ? single : new OverloadGroup(all);

        /// <summary>
        /// A call first infers what the candidates that can infer type arguments take, so
        /// those compete as they would be called.
        /// </summary>
        public IResult Call(Argument[] arguments)
            => Scope.Overloading.Apply(
                [.. candidates.Select(candidate => candidate.Is<IInferable>(out var inferable) ? inferable.Infer(arguments) ?? candidate : candidate)],
                candidate => Scope.CG.Call(candidate, arguments)
            );

        /// <summary>
        /// Indexing a group, as giving generic arguments does, is applied to each candidate,
        /// and the candidates it applies to are a group again: which of them is meant is
        /// what is done with it next, as a call, to say. If it applies to none, the error is
        /// what overloading reports.
        /// </summary>
        public IResult Index(Argument[] arguments)
        {
            var indexed = new List<CompilerObject>();

            foreach (var candidate in candidates)
                if (Scope.CG.Index(candidate, arguments).Ok(out var result))
                    indexed.Add(result);

            return indexed.Count > 0
                ? Result.Ok(Of(indexed))
                : Scope.Overloading.Apply(candidates, candidate => Scope.CG.Index(candidate, arguments));
        }

        /// <summary>
        /// A group goes where one of its candidates does, as a group of methods goes where a
        /// delegate is expected: casting it is applied to each candidate.
        /// </summary>
        public IResult ImplicitCast(CompilerObject type)
            => Scope.Overloading.Apply(candidates, candidate => Scope.CG.ImplicitCast(candidate, type));

        public IResult Cast(CompilerObject targetType)
            => Scope.Overloading.Apply(candidates, candidate => Scope.CG.Cast(candidate, targetType));
    }
}
