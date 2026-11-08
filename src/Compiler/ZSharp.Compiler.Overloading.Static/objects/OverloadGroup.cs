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
    }
}
