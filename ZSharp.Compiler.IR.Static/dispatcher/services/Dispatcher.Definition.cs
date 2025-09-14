namespace ZSharp.Compiler.IRDispatchers.Static
{
    partial class Dispatcher
        : IIRDefinitionInCompiler
        , IIRDefinitionAsCompiler
    {
        bool IIRDefinitionInCompiler.CompileDefinition<Owner>(CompilerObject @object, Owner owner, TargetPlatform? target)
        {
            var result = @base.CompileDefinition(@object, owner, target);

            if (!result && @object.Is<ICompileIRDefinitionIn<Owner>>(out var compile))
            {
                compile.CompileIRDefinition(compiler, owner, target);
                result = true;
            }

            return result;
        }

        Result<T> IIRDefinitionAsCompiler.CompileDefinition<T>(CompilerObject @object, object? target)
        {
            var result = @base.CompileDefinition<T>(@object, target);

            if (result.IsError && @object.Is<ICompileIRDefinitionAs<T>>(out var compile))
                result = compile.CompileIRDefinition(compiler, target);

            return result;
        }
    }
}
