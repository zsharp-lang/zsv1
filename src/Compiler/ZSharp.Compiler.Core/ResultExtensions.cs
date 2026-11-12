namespace ZSharp.Compiler
{
    public static class ResultExtensions
    {
        public static bool IsUnsupported<T>(this IResult<T, Error> result)
            where T : class?
            => result.Error(out var error) && error.IsUnsupported();

        /// <summary>
        /// Whether an error says an operation is unsupported, as its capability.
        /// </summary>
        public static bool IsUnsupported(this Error? error)
            => error is not null && error.Is<Unsupported>(out _);
    }
}
