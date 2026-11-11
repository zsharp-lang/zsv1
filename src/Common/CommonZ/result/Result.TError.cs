namespace CommonZ
{
    public sealed class Result<TResult, TError>
        : IResult<TResult, TError>
        where TResult : class?
        where TError : class
    {
        private readonly bool isOk;
        private readonly TResult? result;
        private readonly TError? error;

        public bool IsOk => isOk;

        public bool IsError => !isOk;

        private Result(bool isOk, TResult? result, TError? error)
        {
            this.isOk = isOk;
            this.result = result;
            this.error = error;
        }

        public static Result<TResult, TError> Ok(TResult result)
            => new(true, result, null);

        public static Result<TResult, TError> Error(TError error)
            => new(false, null, error);

        TResult IResult<TResult, TError>.Unwrap()
            => isOk ? result! : throw new InvalidOperationException(error!.ToString());

        TError IResult<TResult, TError>.UnwrapError()
            => isOk ? throw new InvalidOperationException("Result is Ok.") : error!;

        IResult<R, TError> IResult<TResult, TError>.When<R>(Func<TResult, R> map)
            => isOk
                ? Result<R, TError>.Ok(map(result!))
                : Result<R, TError>.Error(error!);

        IResult<TResult, E> IResult<TResult, TError>.Else<E>(Func<TError, E> map)
            => isOk
                ? Result<TResult, E>.Ok(result!)
                : Result<TResult, E>.Error(map(error!));
    }
}
