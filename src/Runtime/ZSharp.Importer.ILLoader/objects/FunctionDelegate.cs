namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A delegate of a function, made where a delegate is expected, of anything that can
    /// be referred to as a function, as a Z# function can: its function is found for the
    /// target it is lowered for, and the delegate points to it, with no object.
    /// </summary>
    internal sealed class FunctionDelegate(ILLoader loader, Type delegateType, CompilerObject function)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => loader.Class(delegateType);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (Core.Compiler.ModuleScope.IR.CompileReference<LIR.ICallable>(function, target).When(out var callable).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            if (callable is not LIR.Function lowered)
                return Result<LIR.Expression>.Error("Only a function defined in LIR can be made a delegate of.");

            var invoke = delegateType.GetMethod("Invoke")!;

            if (invoke.GetParameters().Length != lowered.Signature.Parameters.Count)
                return Result<LIR.Expression>.Error($"A {delegateType.Name} takes {invoke.GetParameters().Length} arguments, but the function takes {lowered.Signature.Parameters.Count}.");

            var code = new LIR.CLR.ClrCode(new LIR.Signature { ReturnType = loader.Handle(delegateType) })
                .Emit(System.Reflection.Emit.OpCodes.Ldnull)
                .Emit(System.Reflection.Emit.OpCodes.Ldftn, lowered)
                .Emit(System.Reflection.Emit.OpCodes.Newobj, loader.Handle(delegateType.GetConstructor([typeof(object), typeof(nint)])!));

            return Result<LIR.Expression>.Ok(new LIR.Call(code));
        }
    }
}
