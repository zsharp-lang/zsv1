namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A property or field, optionally on an object.
    /// </summary>
    internal sealed class Accessor(
        ILLoader loader,
        CompilerObject? receiver,
        Type type,
        LIR.FunctionHandle? getter,
        LIR.FunctionHandle? setter
    )
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
    {
        CompilerObject ITyped.Type => loader.Class(type);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => getter is null
                ? Result<LIR.Expression>.Error("This member can't be read.")
                : new Call(loader, getter, [.. Receiver()], type).CompileIRExpression(target);

        public IResult Set(CompilerObject value)
            => setter is null
                ? Result.Error("This member can't be written.")
                : Result.Ok(new Call(loader, setter, [.. Receiver(), value], null));

        private IEnumerable<CompilerObject> Receiver()
            => receiver is null ? [] : [receiver];
    }
}
