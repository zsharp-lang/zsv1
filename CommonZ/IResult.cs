public interface IResult<out TResult, out TError>
        where TResult : class?
        where TError : class
{
    public bool IsOk { get; }

    public bool IsError { get; }

    public TResult Unwrap();

    public TError UnwrapError();

    public IResult<TResult, TError> When(Action<TResult> action)
    {
        if (this.Ok(out var result))
            action(result);

        return this;
    }

    public IResult<R, TError> When<R>(Func<TResult, R> map) where R : class;

    public IResult<TResult, TError> Else(Action<TError> action)
    {
        if (this.Error(out var error))
            action(error);

        return this;
    }

    public IResult<TResult, E> Else<E>(Func<TError, E> map) where E : class;
}