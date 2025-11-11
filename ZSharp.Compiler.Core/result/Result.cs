using CommonZ;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public class Result<TResult>
        : IResult<TResult, Error>
        where TResult : class?
    {
        private readonly TResult? result;
        private readonly Error? error;

        [MemberNotNullWhen(true, nameof(result))]
        public bool IsOk => result is not null;

        [MemberNotNullWhen(true, nameof(error))]
        public bool IsError => error is not null;

        private Result(TResult? result, Error? error)
        {
            this.result = result;
            this.error = error;
        }

        public static Result<TResult> Ok(TResult result)
            => new(result, null);

        public static Result<TResult> Error(Error error)
            => new(null, error);

        public static Result<TResult> Error(string message)
            => Error(new ErrorMessage(message));

        public static Result<TResult> Error(params IEnumerable<Error> errors)
            => new(null, new AggregateError(errors));

        TResult IResult<TResult, Error>.Unwrap()
            => result ?? throw new InvalidOperationException(error!.ToString());

        Error IResult<TResult, Error>.UnwrapError()
            => error ?? throw new InvalidOperationException("Result is Ok.");

        IResult<R, Error> IResult<TResult, Error>.When<R>(Func<TResult, R> map)
            => IsOk
                ? Result<R>.Ok(map(result))
                : Result<R>.Error(error!);

        IResult<TResult, E> IResult<TResult, Error>.Else<E>(Func<Error, E> map)
            => IsOk
                ? Result<TResult, E>.Ok(result)
                : Result<TResult, E>.Error(map(error!));
    }
}
