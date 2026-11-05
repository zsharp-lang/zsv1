namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed class Method(LIR.FunctionHandle handle)
        : CompilerObject
        , ICTCallable
        , ICompileIRReference<LIR.ICallable>
    {
        public IResult Call(Argument[] arguments)
            => arguments.Length == handle.Signature.Parameters.Count
                ? Result.Ok(new Call(handle, arguments))
                : Result.Error($"Expected {handle.Signature.Parameters.Count} arguments, but got {arguments.Length}.");

        public IResult<LIR.ICallable, Error> CompileIRReference(object? target)
            => Result<LIR.ICallable>.Ok(handle);
    }
}
