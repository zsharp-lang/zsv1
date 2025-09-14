namespace ZSharp.Compiler.Dispatchers.Proxy
{
    partial class Dispatcher
    {
        public Result ImplicitCast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.ImplicitCast(@object, type);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.ImplicitCast(proxied, type));

            return result;
        }
    }
}
