namespace ZSharp.Compiler.IRDispatchers.Static
{
    partial class Dispatcher
        : IIRReferenceCompiler
    {
        IResult<T, Error> IIRReferenceCompiler.CompileReference<T>(CompilerObject @object, object? target) where T : class
        {
            var result = @base.CompileReference<T>(@object, target);

            if (result.IsError && @object.Is<ICompileIRReference<T>>(out var compile))
                result = compile.CompileIRReference(compiler, target);

            return result;
        }
    }
}
