namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A property or field, optionally on an object. A field that can be assigned also has
    /// an address, so it can be passed by reference as it is.
    /// </summary>
    internal sealed class Accessor(
        CompilerObject? receiver,
        Func<CompilerObject> type,
        LIR.ICallable? getter,
        LIR.ICallable? setter,
        LIR.ICallable? address = null
    )
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
        , IHasAddress
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

        /// <summary>
        /// The code that gives the address of the field, through the function that does.
        /// </summary>
        public IResult<LIR.Expression, Error>? AddressOf(object? target)
            => address is null ? null : new Call(address, [.. Receiver()], null).CompileIRExpression(target);

        private IEnumerable<CompilerObject> Receiver()
            => receiver is null ? [] : [receiver];
    }
}
