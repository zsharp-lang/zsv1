
namespace ZSharp.SourceCompiler
{
    public sealed class DeferredExpression
        : CompilerObject
        , IProxy
    {
        private CompilerObject? value;

        public required Delegate Function { private get; init; }

        public required object[] Arguments { private get; init; }

        IResult<R, Error> IProxy.Apply<R>(Func<CompilerObject, IResult<R, Error>> fn)
        {
            if (value is null)
            {
                var result = Function.DynamicInvoke(Arguments) as IResult;

                if (result is null)
                    return Result<R>.Error("Deferred expression did not return a valid result.");

                if (result.When(out value).Error(out var error))
                    return Result<R>.Error(error);
            }
            
            return fn(value!);
        }
    }
}
