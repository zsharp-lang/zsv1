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
            if (ZSharp.Compiler.Current.IR.CompileReference<LIR.ICallable>(function, target).When(out var callable).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            if (callable is not LIR.Function lowered)
                return Result<LIR.Expression>.Error("Only a function defined in LIR can be made a delegate of.");

            var invoke = definition.GetMethod("Invoke")!;

            // A function bound to an object takes it first, and the delegate is closed over it.
            LIR.Expression? receiver = null;
            if (Receiver(function) is { } @object)
                if (ZSharp.Compiler.Current.IR.CompileExpression(@object, target).When(out receiver).Error(out error))
                    return Result<LIR.Expression>.Error(error);

            var taken = lowered.Signature.Parameters.Count - (receiver is null ? 0 : 1);
            if (invoke.GetParameters().Length != taken)
                return Result<LIR.Expression>.Error($"A {definition.Name} takes {invoke.GetParameters().Length} arguments, but the function takes {taken}.");

            if (ZSharp.Compiler.Current.IR.CompileType(delegateType, target).When(out var type).Error(out error))
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

            if (receiver is null)
            {
                var unbound = new LIR.CLR.ClrCode(new LIR.Signature { ReturnType = type })
                    .Emit(System.Reflection.Emit.OpCodes.Ldnull)
                    .Emit(System.Reflection.Emit.OpCodes.Ldftn, lowered)
                    .Emit(System.Reflection.Emit.OpCodes.Newobj, constructor);

                return Result<LIR.Expression>.Ok(new LIR.Call(unbound));
            }

            var signature = new LIR.Signature { ReturnType = type };
            signature.Parameters.Add(new LIR.Parameter(lowered.Signature.Parameters[0].Type));

            var closed = new LIR.CLR.ClrCode(signature)
                .Emit(System.Reflection.Emit.OpCodes.Ldftn, lowered)
                .Emit(System.Reflection.Emit.OpCodes.Newobj, constructor);

            return Result<LIR.Expression>.Ok(new LIR.Call(closed, [receiver]));
        }

        private static CompilerObject? Receiver(CompilerObject function)
            => function.Is<ICTBound>(out var bound) ? bound.Receiver : null;
    }
}
