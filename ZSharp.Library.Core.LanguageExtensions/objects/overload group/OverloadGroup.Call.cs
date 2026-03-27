using ZSharp.Compiler;

namespace Core.LanguageExtensions.Objects
{
    partial class OverloadGroup
        : ICTCallable
    {
        IResult<CompilerObject, Error> ICTCallable.Call(Compiler compiler, Argument[] arguments)
        {
            var matched =
                overloads
                .Select(overload => compiler.CG.Call(overload, arguments))
                .Where(result => result.IsOk)
                .Select(result => result.Unwrap())
                .ToArray();

            if (matched.Length == 0)
                return Result.Error($"No overloads matched the given arguments for overload {Name}.");
            if (matched.Length > 1)
                return Result.Error($"Multiple overloads matched the given arguments for overload {Name}.");

            return Result.Ok(matched[0]);
        }
    }
}
