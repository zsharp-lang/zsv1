namespace ZSharp.Compiler.CGDispatchers.Proxy
{
    partial class Dispatcher
    {
        public IResult Index(CompilerObject @object, Argument[] arguments)
        {
            var result = @base.GetIndex(@object, arguments);

            if (result.IsError && @object.Is<ICOProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Index(proxied, arguments));

            return result;
        }

        public IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
        {
            var result = @base.GetIndex(@object, arguments);

            if (result.IsError && @object.Is<ICOProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Index(proxied, arguments, value));

            return result;
        }
    }
}
