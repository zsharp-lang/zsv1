using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A function of a module defined in Z#, called with its arguments, as its module is
    /// lowered for the target. It is referred to as its function, so it is a delegate where
    /// one is expected.
    /// </summary>
    internal sealed class ModuleFunction(ILLoader loader, Package.DotNet.Method node, DefinedModule module)
        : CompilerObject
        , ICTCallable
        , ICompileIRReference<LIR.ICallable>
    {
        private IResult<LIR.ICallable, Error> Function(object? target)
            => module.For(target).When(out var lowered).Error(out var error)
                ? Result<LIR.ICallable>.Error(error)
                : Result<LIR.ICallable>.Ok(lowered!.Functions[node]);

        public IResult<LIR.ICallable, Error> CompileIRReference(object? target)
            => Function(target);

        public IResult Call(Argument[] arguments)
        {
            if (Arguments.Parameters(node.Parameters) is not { } matching || Arguments.Match(matching, arguments) is not { } values)
                return Result.Error($"{node.Name} can't be called with these arguments.");

            return Result.Ok(new Call(
                Function,
                values,
                node.ReturnType is { } returned ? () => Scope.HIRLoader.LoadType(returned).Unwrap() : null
            ));
        }
    }

    /// <summary>
    /// A <c>let</c> of a module defined in Z#: a global of its LIR module, as the module is
    /// lowered for the target.
    /// </summary>
    internal sealed class ModuleGlobal(DefinedModule module, Package.DotNet.Field field)
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
    {
        private CompilerObject Type() => Scope.HIRLoader.LoadType(field.Type).Unwrap();

        CompilerObject ITyped.Type => Type();

        private IResult<LIR.Global, Error> Global(object? target)
            => module.For(target).When(out var lowered).Error(out var error)
                ? Result<LIR.Global>.Error(error)
                : Result<LIR.Global>.Ok(lowered!.Globals[field]);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Global(target).When(out var global).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.GetGlobal(global!));

        public IResult Set(CompilerObject value)
            => Scope.CG.ImplicitCast(value, Type()).Ok(out var converted)
                ? Result.Ok(new SetGlobal(this, converted, Type))
                : Result.Error("The value doesn't match the type of this variable.");

        private sealed class SetGlobal(ModuleGlobal global, CompilerObject value, Func<CompilerObject> type)
            : CompilerObject
            , ICompileIRExpression
            , ITyped
        {
            CompilerObject ITyped.Type => type();

            public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            {
                if (global.Global(target).When(out var lowered).Error(out var error))
                    return Result<LIR.Expression>.Error(error);

                return Scope.IR.CompileExpression(value, target).When(out var result).Error(out error)
                    ? Result<LIR.Expression>.Error(error)
                    : Result<LIR.Expression>.Ok(new LIR.Sequence(
                        new LIR.Block(new LIR.SetGlobal(lowered!, result!)),
                        new LIR.GetGlobal(lowered!)
                    ));
            }
        }
    }
}
