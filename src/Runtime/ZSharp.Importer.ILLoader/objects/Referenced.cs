namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A parameter that takes a reference, <c>Ref[T]</c> or <c>Out[T]</c>, as the code it
    /// belongs to uses it: a variable of <c>T</c>. Reading it reads what the reference
    /// refers to, assigning it assigns that, and passing it on by reference passes the
    /// reference it holds.
    /// </summary>
    internal sealed class Referenced(ILLoader loader, Variable reference, Type element)
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
        , IHasAddress
    {
        CompilerObject ITyped.Type => loader.Class(element);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => new Call(loader.LoadIndirect(element), [reference], () => loader.Class(element)).CompileIRExpression(target);

        public IResult Set(CompilerObject value)
            => Core.Compiler.ModuleScope.CG.ImplicitCast(value, loader.Class(element)).Ok(out var converted)
                ? Result.Ok(new Call(loader.StoreIndirect(element), [reference, converted], null))
                : Result.Error($"The value doesn't go to {element.Name}.");

        public IResult<LIR.Expression, Error>? AddressOf(object? target)
            => reference.CompileIRExpression(target);
    }
}
