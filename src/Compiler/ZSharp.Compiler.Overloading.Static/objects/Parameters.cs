using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Compiler
{
    /// <summary>
    /// How a candidate with parameters compares with another one, without looking at it:
    /// it is at least as specific if the other one takes whatever it takes, which is asked
    /// by calling the other one with values of its parameter types.
    /// </summary>
    public static class Parameters
    {
        public static bool AreAsSpecificAs(IEnumerable<CompilerObject> types, CompilerObject other)
            => Scope.CG.Call(other, [.. types.Select(type => new Argument(new Placeholder(type)))]).IsOk;

        /// <summary>
        /// A value that only has a type. Nothing compiles it.
        /// </summary>
        private sealed class Placeholder(CompilerObject type)
            : CompilerObject
            , ITyped
        {
            public CompilerObject Type => type;
        }
    }
}
