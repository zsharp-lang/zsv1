namespace ZSharp.Compiler
{
    public static class ResultExtensions
    {
        public static bool IsUnsupported<T>(this IResult<T, Error> result)
            where T : class?
            => result.Error(out var error) && error is UnsupportedError;
    }
}
