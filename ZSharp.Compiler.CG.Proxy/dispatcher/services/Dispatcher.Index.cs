namespace ZSharp.Compiler.CGDispatchers.Proxy
{
    partial class Dispatcher
    {
        public Result Index(CompilerObject @object, Argument[] arguments)
        {
            var result = @base.GetIndex(@object, arguments);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Index(proxied, arguments));

            return result;
        }

        public Result Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
        {
            var result = @base.GetIndex(@object, arguments);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Index(proxied, arguments, value));

            return result;
        }
    }
}
