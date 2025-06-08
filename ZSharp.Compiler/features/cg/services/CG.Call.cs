namespace ZSharp.Compiler
{
    public partial struct CG
    {
        public CompilerObjectResult Call(CompilerObject @object, Argument_NEW<CompilerObject>[] arguments)
        {
            var result = CompilerObjectResult.Error("Object does not support calling");

            if (@object is ICTCallable_NEW ctCallable)
                result = ctCallable.Call(compiler, arguments);

            if (result.IsOk) return result;

            if (RuntimeDescriptor(@object, out var runtimeDescriptor)
                && runtimeDescriptor is IRTCallable_NEW rtCallable
            )
                result = rtCallable.Call(compiler, @object, arguments);

            if (result.IsOk) return result;

            return result;
        }
    }
}
