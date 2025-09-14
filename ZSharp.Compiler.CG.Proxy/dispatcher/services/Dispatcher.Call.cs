namespace ZSharp.Compiler.CGDispatchers.Proxy
{
    partial class Dispatcher
    {
        public Result Call(CompilerObject callee, Argument[] arguments)
        {
            var result = @base.Call(callee, arguments);

            if (result.IsError && callee.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Call(proxied, arguments));

            return result;
        }
    }
}
