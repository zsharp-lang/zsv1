namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A delegate of a function, made where a delegate is expected, of anything that can
    /// be referred to as a function, as a Z# function can: its function is found for the
    /// target it is lowered for, and the delegate points to it, with no object. The delegate
    /// type may be an instance of a generic one over types that aren't the CLR's yet, as
    /// <c>Func[Function, IResult]</c> is, whose constructor is its definition's, on the
    /// instance as the target lowers it.
    /// </summary>
    internal sealed class FunctionDelegate(ILLoader loader, CompilerObject delegateType, Type definition, CompilerObject function)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => delegateType;

        /// <summary>
        /// Whether a CLR type is a delegate type a function can be a delegate of.
        /// </summary>
        public static bool Delegates(Type type)
            => typeof(Delegate).IsAssignableFrom(type) && type != typeof(Delegate) && type != typeof(MulticastDelegate);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (Core.Compiler.ModuleScope.IR.CompileReference<LIR.ICallable>(function, target).When(out var callable).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            if (callable is not LIR.Function lowered)
                return Result<LIR.Expression>.Error("Only a function defined in LIR can be made a delegate of.");

            var invoke = definition.GetMethod("Invoke")!;

            if (invoke.GetParameters().Length != lowered.Signature.Parameters.Count)
                return Result<LIR.Expression>.Error($"A {definition.Name} takes {invoke.GetParameters().Length} arguments, but the function takes {lowered.Signature.Parameters.Count}.");

            if (Core.Compiler.ModuleScope.IR.CompileType(delegateType, target).When(out var type).Error(out error))
                return Result<LIR.Expression>.Error(error);

            var constructor = loader.Handle(definition.GetConstructor([typeof(object), typeof(nint)])!);

            // An instance's constructor is its definition's, on the instance.
            if (type is LIR.GenericInstance instance)
                constructor = new LIR.CLR.ClrMethod(constructor.Signature)
                {
                    Assembly = constructor.Assembly,
                    Module = constructor.Module,
                    Token = constructor.Token,
                    Name = constructor.Name,
                    DeclaringType = instance,
                    IsConstructor = true,
                    Parameters = constructor.Parameters,
                };

            var code = new LIR.CLR.ClrCode(new LIR.Signature { ReturnType = type })
                .Emit(System.Reflection.Emit.OpCodes.Ldnull)
                .Emit(System.Reflection.Emit.OpCodes.Ldftn, lowered)
                .Emit(System.Reflection.Emit.OpCodes.Newobj, constructor);

            return Result<LIR.Expression>.Ok(new LIR.Call(code));
        }
    }
}
