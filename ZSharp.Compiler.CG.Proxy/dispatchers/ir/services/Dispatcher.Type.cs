namespace ZSharp.Compiler.IRDispatchers.Proxy
{
    partial class Dispatcher
        : IIRTypeCompiler
    {
        public IResult<ZSharp.IR.IType, Error> CompileType(CompilerObject @object, object? target)
        {
            var result = @base.CompileType(@object, target);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => IR.CompileType(proxied, target));

            return result;
        }

        IResult<T, Error> IIRTypeCompiler.CompileType<T>(CompilerObject @object, object? target)
        {
            var result = @base.CompileTypeAs<T>(@object, target);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => IR.CompileTypeAs<T>(proxied, target));

            return result;
        }
    }
}
