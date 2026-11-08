namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A property or field, optionally on an object.
    /// </summary>
    internal sealed class Accessor(
        CompilerObject? receiver,
        Func<CompilerObject> type,
        LIR.ICallable? getter,
        LIR.ICallable? setter
    )
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
    {
        CompilerObject ITyped.Type => type();

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => getter is null
                ? Result<LIR.Expression>.Error("This member can't be read.")
                : new Call(getter, [.. Receiver()], type).CompileIRExpression(target);

        public IResult Set(CompilerObject value)
        {
            if (setter is null)
                return Result.Error("This member can't be written.");

            return Core.Compiler.ModuleScope.CG.ImplicitCast(value, type()).Ok(out var converted)
                ? Result.Ok(new Call(setter, [.. Receiver(), converted], null))
                : Result.Error("The value doesn't match the type of this member.");
        }

        private IEnumerable<CompilerObject> Receiver()
            => receiver is null ? [] : [receiver];
    }
}
