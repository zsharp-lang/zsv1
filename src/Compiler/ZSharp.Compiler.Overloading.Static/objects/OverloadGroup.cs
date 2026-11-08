using Scope = Core.Compiler.ModuleScope;

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
        public static CompilerObject Of(IEnumerable<CompilerObject> candidates)
            => candidates.ToArray() is var all && all is [var single] ? single : new OverloadGroup(all);

        public IResult Call(Argument[] arguments)
            => Scope.Overloading.Apply(candidates, candidate => Scope.CG.Call(candidate, arguments));

        /// <summary>
        /// Indexing a group, as giving generic arguments does, is applied to each candidate.
        /// </summary>
        public IResult Index(Argument[] arguments)
            => Scope.Overloading.Apply(candidates, candidate => Scope.CG.Index(candidate, arguments));

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
