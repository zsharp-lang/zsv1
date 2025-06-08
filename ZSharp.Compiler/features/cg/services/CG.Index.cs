namespace ZSharp.Compiler
{
    public partial struct CG
    {
        public CompilerObjectResult Index(CompilerObject @object, Argument_NEW<CompilerObject>[] arguments)
        {
            var result = CompilerObjectResult.Error("Object does not support get index");

            if (@object is ICTGetIndex_NEW ctGet)
                result = ctGet.Index(compiler, arguments);

            if (result.IsOk) return result;

            if (RuntimeDescriptor(@object, out var runtimeDescriptor)
                && runtimeDescriptor is IRTGetIndex_NEW rtGet
            )
                result = rtGet.Index(compiler, @object, arguments);

            if (result.IsOk) return result;

            if (@object is IObjectWrapper<CompilerObjectResult> wrapper && This(out var cg)) // TODO: this is in testing
                result = wrapper.MapWrapped(compiler, wrapped => cg.Index(wrapped, arguments));

            return result;
        }

        public CompilerObjectResult Index(CompilerObject @object, Argument_NEW<CompilerObject>[] arguments, CompilerObject value)
        {
            var result = CompilerObjectResult.Error("Object does not support set index");

            if (@object is ICTSetIndex_NEW ctGet)
                result = ctGet.Index(compiler, arguments, value);

            if (result.IsOk) return result;

            if (RuntimeDescriptor(@object, out var runtimeDescriptor)
                && runtimeDescriptor is IRTSetIndex_NEW rtGet
            )
                result = rtGet.Index(compiler, @object, arguments, value);

            if (result.IsOk) return result;

            return result;
        }
    }
}
