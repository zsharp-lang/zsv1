using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// A local. A definition is loaded once, so every use shares one LIR local.
    /// </summary>
    internal sealed class Local(HIR.Code.Local node)
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
    {
        private LIR.Local? local;

        CompilerObject ITyped.Type => Scope.HIRLoader.LoadType(node.Type).Unwrap();

        public IResult<LIR.Local, Error> Compile(object? target)
        {
            if (local is not null)
                return Result<LIR.Local>.Ok(local);

            if (Scope.HIRLoader.LoadType(node.Type).When(out var type).Error(out var error))
                return Result<LIR.Local>.Error(error);

            if (Scope.IR.CompileType(type!, target).When(out var compiled).Error(out error))
                return Result<LIR.Local>.Error(error);

            return Result<LIR.Local>.Ok(local = new(compiled!));
        }

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Compile(target).When(out var compiled).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.GetLocal(compiled!));

        public IResult Set(CompilerObject value)
            => Result.Ok(new SetLocal(this, value));
    }

    internal sealed class SetLocal(Local local, CompilerObject value)
        : CompilerObject
        , ICompileIRExpression
    {
        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (local.Compile(target).When(out var compiled).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            if (Scope.IR.CompileExpression(value, target).When(out var result).Error(out error))
                return Result<LIR.Expression>.Error(error);

            return Result<LIR.Expression>.Ok(new LIR.Sequence(
                new LIR.Block(new LIR.SetLocal(compiled!, result!)),
                new LIR.GetLocal(compiled!)
            ));
        }
    }
}
