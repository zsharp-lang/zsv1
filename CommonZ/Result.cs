using System.Diagnostics.CodeAnalysis;

namespace CommonZ
{
    public class Result<TResult, TError>
        where TResult : class?
        where TError : class
    {
        protected readonly TResult? result;
        protected readonly TError? error;

        [MemberNotNullWhen(true, nameof(result))]
        public bool IsOk => result is not null;

        [MemberNotNullWhen(true, nameof(error))]
        public bool IsError => error is not null;

        protected Result(TResult? result, TError? error)
        {
            this.result = result;
            this.error = error;
        }

        public static Result<TResult, TError> Ok(TResult result)
            => new(result, null);

        public static Result<TResult, TError> Error(TError error)
            => new(null, error);

        public bool Ok([NotNullWhen(true)] out TResult? result)
            => (result = this.result) is not null;

        public bool Error([NotNullWhen(true)] out TError? error)
            => (error = this.error) is not null;

        public TResult Unwrap()
            => result ?? throw new InvalidOperationException(error!.ToString());

        public Result<TResult, TError> When(Action<TResult> action)
        {
            if (IsOk)
                action(result);

            return this;
        }

        public Result<R, TError> When<R>(Func<TResult, R> map)
            where R : class
            => IsOk
                ? Result<R, TError>.Ok(map(result))
                : Result<R, TError>.Error(error!);

        public Result<TResult, TError> When(out TResult? result)
        {
            result = this.result;
            return this;
        }

        public Result<TResult, TError> Else(Action<TError> action)
        {
            if (IsError)
                action(error);

            return this;
        }

        public Result<TResult, E> Else<E>(Func<TError, E> map)
            where E : class
            => IsOk
                ? Result<TResult, E>.Ok(result)
                : Result<TResult, E>.Error(map(error!));

        public Result<TResult, TError> Else(out TError? error)
        {
            error = this.error;
            return this;
        }
    }
}
