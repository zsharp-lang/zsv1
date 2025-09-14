namespace ZSharp.Compiler.IRDispatchers.Static
{
    partial class Dispatcher
        : IIRReferenceCompiler
    {
        Result<T> IIRReferenceCompiler.CompileReference<T>(CompilerObject @object) where T : class
        {
            var result = @base.CompileReference<T>(@object);

            if (result.IsError && @object.Is<ICompileIRReference<T>>(out var compile))
                result = compile.CompileIRReference(compiler);

            return result;
        }
    }
}
