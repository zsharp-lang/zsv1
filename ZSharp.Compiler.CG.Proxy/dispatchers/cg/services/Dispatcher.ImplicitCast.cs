namespace ZSharp.Compiler.CGDispatchers.Proxy
{
    partial class Dispatcher
    {
        public IResult ImplicitCast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.ImplicitCast(@object, type);

            if (result.IsError && @object.Is<ICOProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.ImplicitCast(proxied, type));

            return result;
        }
    }
}
