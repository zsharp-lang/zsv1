using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    internal static class HelperFunctions
    {
        public static bool CombineErrorResults<T>(
            IEnumerable<Result<T, Error>> results,
            [NotNullWhen(true)] out CombinedCompilationError? error
        )
            where T : class
        {
            var errors =
                results
                .Where(r => r.IsError)
                .Select(r => r.Error(out var e) ? e : throw new())
                .SelectMany(e => e switch
                {
                    CompilationError compilationError => [compilationError],
                    CombinedCompilationError combined => combined.Errors,
                    _ => throw new("Unknown error type: " + e.GetType().Name)
                })
                .ToArray();

            error = errors.Length > 0 ? new(errors) : null;

            return errors.Length > 0;
        }
    }
}
