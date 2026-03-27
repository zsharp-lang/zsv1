namespace ZSharp.Compiler.TSDispatchers.Proxy
{
    partial class Dispatcher
    {
        public IResult TypeOf(CompilerObject @object)
        {
            var result = @base.TypeOf(@object);

            if (result.IsError && @object.Is<ICOProxy>(out var proxy))
                result = proxy.Apply(proxied => compiler.TS.TypeOf(proxied));

            return result;
        }
    }
}
