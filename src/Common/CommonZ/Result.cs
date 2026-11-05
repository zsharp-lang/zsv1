using System.Diagnostics.CodeAnalysis;

namespace CommonZ
{
    public sealed class Result<TResult, TError>
        : IResult<TResult, TError>
        where TResult : class?
        where TError : class
    {
        private readonly TResult? result;
        private readonly TError? error;

        [MemberNotNullWhen(true, nameof(result))]
        public bool IsOk => result is not null;

        [MemberNotNullWhen(true, nameof(error))]
        public bool IsError => error is not null;

        private Result(TResult? result, TError? error)
        {
            this.result = result;
            this.error = error;
        }

        public static Result<TResult, TError> Ok(TResult result)
            => new(result, null);

        public static Result<TResult, TError> Error(TError error)
            => new(null, error);

        TResult IResult<TResult, TError>.Unwrap()
            => result ?? throw new InvalidOperationException(error!.ToString());

        TError IResult<TResult, TError>.UnwrapError()
            => error ?? throw new InvalidOperationException("Result is Ok");

        IResult<R, TError> IResult<TResult, TError>.When<R>(Func<TResult, R> map)
            => IsOk
                ? Result<R, TError>.Ok(map(result))
                : Result<R, TError>.Error(error!);

        IResult<TResult, E> IResult<TResult, TError>.Else<E>(Func<TError, E> map)
            => IsOk
                ? Result<TResult, E>.Ok(result)
                : Result<TResult, E>.Error(map(error!));
    }
}
