namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A variable passed by reference. A variable that has an address of its own, as a
    /// local or a field has, is passed by it. One that has none, as a local of a document
    /// or a property, is passed through a temporary that the call assigns back to it
    /// once it returns, so the callee sees one variable either way.
    /// </summary>
    internal sealed class ByAddress(ILLoader loader, CompilerObject variable, Type element)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        public CompilerObject Variable => variable;

        public Type Element => element;

        public LIR.Type ElementType => loader.Handle(element);

        public CompilerObject ElementClass => loader.Class(element);

        public LIR.Type AddressType => loader.Handle(element.MakeByRefType());

        CompilerObject ITyped.Type => loader.Ref(element, isOut: false);

        /// <summary>
        /// The variable's address, or <see langword="null"/> with its value if it has none.
        /// </summary>
        public Error? Lower(object? target, out LIR.Expression? address, out LIR.Expression? value)
        {
            address = value = null;

            if (variable.Is<IHasAddress>(out var addressable) && addressable.AddressOf(target) is { } own)
                return own.When(out address).Error(out var failure) ? failure : null;

            if (Core.Compiler.ModuleScope.IR.CompileExpression(variable, target).When(out var compiled).Error(out var error))
                return error;

            if (LIR.CLR.AddressOf.Has(compiled!))
                address = new LIR.CLR.AddressOf(compiled!, AddressType);
            else
                value = compiled;

            return null;
        }

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (Lower(target, out var address, out _) is { } error)
                return Result<LIR.Expression>.Error(error);

            return address is null
                ? Result<LIR.Expression>.Error("This variable has no address, so it can only be passed by reference to a call.")
                : Result<LIR.Expression>.Ok(address);
        }
    }

    /// <summary>
    /// The temporary a variable with no address is passed through.
    /// </summary>
    internal sealed class Temporary(LIR.Local local, CompilerObject type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.GetLocal(local));
    }
}
