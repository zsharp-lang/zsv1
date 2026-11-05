namespace CommonZ
{
    /// <summary>
    /// Shorthand for creating a <see cref="Result{TResult, TError}"/> whose error
    /// is an <see cref="CommonZ.Error"/>.
    /// </summary>
    public static class Result<TResult>
        where TResult : class?
    {
        public static Result<TResult, Error> Ok(TResult result)
            => Result<TResult, Error>.Ok(result);

        public static Result<TResult, Error> Error(Error error)
            => Result<TResult, Error>.Error(error);

        public static Result<TResult, Error> Error(string message)
            => Error(new ErrorMessage(message));

        public static Result<TResult, Error> Error(params IEnumerable<Error> errors)
            => Error(new AggregateError(errors));
    }
}
