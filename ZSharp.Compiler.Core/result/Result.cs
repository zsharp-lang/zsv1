using CommonZ;

namespace ZSharp.Compiler
{
    public class Result<TResult>
        : Result<TResult, Error>
        where TResult : class?
    {
        protected Result(TResult? result, Error? error)
            : base(result, error)
        {

        }

        public new static Result<TResult> Ok(TResult result)
            => new(result, null);

        public new static Result<TResult> Error(Error error)
            => new(null, error);

        public static Result<TResult> Error(string message)
            => Error(new ErrorMessage(message));

        public new Result<TResult> When(Action<TResult> action)
        {
            if (IsOk)
                action(Unwrap());

            return this;
        }

        public new Result<R> When<R>(Func<TResult, R> map)
            where R : class?
            => IsOk
                ? Result<R>.Ok(map(result))
                : Result<R>.Error(error!);

        public Result<R> When<R>(Func<TResult, Result<R>> map)
            where R : class?
            => IsOk
                ? map(result)
                : Result<R>.Error(error!);

        public new Result<TResult> When(out TResult? result)
        {
            result = this.result;
            return this;
        }

        public new Result<TResult> Else(Action<Error> action)
        {
            if (IsError)
                action(error);

            return this;
        }

        public new Result<TResult, E> Else<E>(Func<Error, E> map)
            where E : class
            => IsOk
                ? Result<TResult, E>.Ok(result)
                : Result<TResult, E>.Error(map(error!));

        public new Result<TResult> Else(out Error? error)
        {
            error = this.error;
            return this;
        }
    }
}
