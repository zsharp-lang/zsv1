using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Several members of one name, as overloading groups them: what the IL importer gives
    /// for a name that has more than one, and calls when it calls one of them.
    /// </summary>
    internal static class Candidates
    {
        public static IResult Of(IEnumerable<CompilerObject> candidates)
            => Scope.Overloading.Group([.. candidates]);

        public static IResult Call(IEnumerable<CompilerObject> candidates, Argument[] arguments)
            => Of(candidates).When(out var group).Error(out var error)
                ? Result.Error(error)
                : Scope.CG.Call(group!, arguments);
    }
}
