namespace ZSharp.Compiler.CGDispatchers.Proxy
{
    partial class Dispatcher
    {
        public IResult<CastResult, Error> Cast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.Cast(@object, type);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Cast(proxied, type));

            return result;
        }
    }
}
