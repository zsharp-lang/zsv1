namespace ZSharp.Compiler
{
    public partial struct CG
    {
        public CompilerObjectResult Get(CompilerObject @object)
        {
            var result = CompilerObjectResult.Error("Object does not support get");

            if (@object is ICTGet ctGet)
                result = ctGet.Get(compiler);

            if (result.IsOk) return result;

            if (RuntimeDescriptor(@object, out var runtimeDescriptor)
                && runtimeDescriptor is IRTGet rtGet
            )
                result = rtGet.Get(compiler, @object);

            if (result.IsOk) return result;

            return result;
        }

        public CompilerObjectResult Set(CompilerObject @object, CompilerObject value)
        {
            var result = CompilerObjectResult.Error("Object does not support set");

            if (@object is ICTSet ctSet)
                result = ctSet.Set(compiler, value);

            if (result.IsOk) return result;

            if (RuntimeDescriptor(@object, out var runtimeDescriptor)
                && runtimeDescriptor is IRTSet rtSet
            )
                result = rtSet.Set(compiler, @object, value);

            if (result.IsOk) return result;

            return result;
        }
    }
}
