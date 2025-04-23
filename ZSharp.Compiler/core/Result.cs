using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public sealed class Result<TResult, TError>
        where TResult : class
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

        public bool Ok([NotNullWhen(true)] out TResult? result)
            => (result = this.result) is not null;

        public bool Error([NotNullWhen(true)] out TError? error)
            => (error = this.error) is not null;

        public TResult Unwrap()
            => result ?? throw new InvalidOperationException();

        public Result<TResult, TError> When(Action<TResult> action)
        {
            if (IsOk)
                action(result);

            return this;
        }

        public Result<TResult, TError> Else(Action<TError> action)
        {
            if (IsError)
                action(error);

            return this;
        }
    }
}
