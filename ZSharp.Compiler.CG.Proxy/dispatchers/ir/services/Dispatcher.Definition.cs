namespace ZSharp.Compiler.IRDispatchers.Proxy
{
    partial class Dispatcher
        : IIRDefinitionInCompiler
        , IIRDefinitionAsCompiler
    {
        bool IIRDefinitionInCompiler.CompileDefinition<Owner>(CompilerObject @object, Owner owner, TargetPlatform? target)
        {
            var result = @base.CompileDefinition(@object, owner, target);

            if (!result && @object.Is<IProxy>(out var proxy))
            { 
                proxy.Apply(proxied => {
                    IR.CompileDefinition(proxied, owner, target);
                    return Result.Ok(null!);
                });
                result = true;
            }

            return result;
        }

        IResult<T, Error> IIRDefinitionAsCompiler.CompileDefinition<T>(CompilerObject @object, object? target)
        {
            var result = @base.CompileDefinition<T>(@object, target);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => IR.CompileDefinition<T>(proxied, target));

            return result;
        }
    }
}
