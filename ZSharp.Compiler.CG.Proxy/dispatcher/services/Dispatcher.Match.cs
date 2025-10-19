namespace ZSharp.Compiler.CGDispatchers.Proxy
{
    partial class Dispatcher
    {
        public IResult<MatchResult, Error> Match(CompilerObject @object, CompilerObject pattern)
        {
            var result = @base.Match(@object, pattern);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Match(proxied, pattern));

            return result;
        }
    }
}
