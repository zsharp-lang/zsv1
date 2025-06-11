namespace ZSharp.ZSSourceCompiler
{
    public sealed class CombinedCompilationError(
        params CompilationError[] errors
    ) : Error
    {
        public List<CompilationError> Errors { get; } = [.. errors];
    }
}
