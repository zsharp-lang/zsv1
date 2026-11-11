using CommonZ;
using System.Diagnostics.CodeAnalysis;

public static class IResultExtensions
{
    public static bool Ok<TResult, TError>(
        this IResult<TResult, TError> result,
        [MaybeNullWhen(false)] out TResult output
    )
        where TResult : class?
        where TError : class
    {
        output = result.IsOk ? result.Unwrap() : null;
        return result.IsOk;
    }

    public static bool Error<TResult, TError>(
        this IResult<TResult, TError> result,
        [NotNullWhen(true)] out TError? error
    )
        where TResult : class?
        where TError : class
        => (error = result.IsError ? result.UnwrapError() : null) is not null;

    public static IResult<TResult, TError> When<TResult, TError>(
        this IResult<TResult, TError> result,
        out TResult? output
    )
        where TResult : class?
        where TError : class
    {
        output = result.IsOk ? result.Unwrap() : null;
        return result;
    }

    public static IResult<TResult, TError> Else<TResult, TError>(
        this IResult<TResult, TError> result,
        out TError? error
    )
        where TResult : class?
        where TError : class
    {
        error = result.IsError ? result.UnwrapError() : null;
        return result;
    }

    public static IResult<R, TError> When<TResult, TError, R>(
        this IResult<TResult, TError> result,
        Func<TResult, IResult<R, TError>> map
    )
        where TResult : class?
        where TError : class
        where R : class
    {
        if (result.IsError) return result.When(_ => (null as R)!);

        return map(result.Unwrap()).Else(e => e);
    }

    //public static IResult<TResult, TError> Else<TResult, TError>(
    //    this IResult<TResult, TError> result,
    //    Action<TError> action
    //)
    //    where TResult : class?
    //    where TError : class
    //{
    //    if (result.IsError)
    //        action(result.UnwrapError());

    //    return result;
    //}
}