using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// Several objects under one name. An operation is applied to each, and the one it
    /// succeeds on is the result.
    /// </summary>
    internal sealed class OverloadGroup(CompilerObject[] overloads)
        : CompilerObject
        , ICTCallable
    {
        public IResult Call(Argument[] arguments)
            => Choose(overloads.Select(overload => Scope.CG.Call(overload, arguments)));

        private static IResult Choose(IEnumerable<IResult> results)
            => results.Where(result => result.IsOk).ToArray() switch
            {
                [var result] => result,
                [] => Result.Error("No overload matches."),
                var several => Result.Error($"{several.Length} overloads match."),
            };
    }
}
