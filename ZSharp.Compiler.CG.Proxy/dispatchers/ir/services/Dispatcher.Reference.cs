namespace ZSharp.Compiler.IRDispatchers.Proxy
{
    partial class Dispatcher
        : IIRReferenceCompiler
    {
        IResult<T, Error> IIRReferenceCompiler.CompileReference<T>(CompilerObject @object, object? target) where T : class
        {
            var result = @base.CompileReference<T>(@object, target);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => IR.CompileReference<T>(proxied, target));

            return result;
        }
    }
}
