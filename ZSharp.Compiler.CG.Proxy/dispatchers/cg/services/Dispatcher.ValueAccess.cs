namespace ZSharp.Compiler.CGDispatchers.Proxy
{
    partial class Dispatcher
    {
        public IResult Get(CompilerObject @object)
        {
            var result = @base.Get(@object);

            if (result.IsError && @object.Is<ICOProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Get(proxied));

            return result;
        }

        public IResult Set(CompilerObject @object, CompilerObject value)
        {
            var result = @base.Set(@object, value);

            if (result.IsError && @object.Is<ICOProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Set(proxied, value));

            return result;
        }
    }
}
