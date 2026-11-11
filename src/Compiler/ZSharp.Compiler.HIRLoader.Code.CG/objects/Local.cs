using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// A local. A definition is loaded once, so every use shares one LIR local. Declaring
    /// it is where it takes its initial value.
    /// </summary>
    internal sealed class Local(CompilerObject type, CompilerObject? init)
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ICTDeclare
        , ITyped
    {
        private LIR.Local? local;

        CompilerObject ITyped.Type => type;

        public IResult<LIR.Local, Error> Compile(object? target)
        {
            if (local is not null)
                return Result<LIR.Local>.Ok(local);

            if (Scope.IR.CompileType(type, target).When(out var compiled).Error(out var error))
                return Result<LIR.Local>.Error(error);

            return Result<LIR.Local>.Ok(local = new(compiled!));
        }

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Compile(target).When(out var compiled).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.GetLocal(compiled!));

        public IResult Set(CompilerObject value)
            => Scope.CG.ImplicitCast(value, type).Ok(out var converted)
                ? Result.Ok(new SetLocal(this, converted))
                : Result.Error("The value doesn't match the type of this local.");

        public IResult Declare()
        {
            if (init is null)
                return Result.Ok(new DeclareLocal(this, null));

            return Scope.CG.ImplicitCast(init, type).Ok(out var converted)
                ? Result.Ok(new DeclareLocal(this, converted))
                : Result.Error("The initial value doesn't match the type of this local.");
        }
    }

    internal sealed class DeclareLocal(Local local, CompilerObject? value)
        : CompilerObject
        , ICompileIRStatement
    {
        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
        {
            if (local.Compile(target).When(out var compiled).Error(out var error))
                return Result<LIR.Statement>.Error(error);

            var block = new LIR.Block();

            block.Locals.Add(compiled!);

            if (value is null)
                return Result<LIR.Statement>.Ok(block);

            if (Scope.IR.CompileExpression(value, target).When(out var result).Error(out error))
                return Result<LIR.Statement>.Error(error);

            block.Statements.Add(new LIR.SetLocal(compiled!, result!));

            return Result<LIR.Statement>.Ok(block);
        }
    }

    internal sealed class SetLocal(Local local, CompilerObject value)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => ((ITyped)local).Type;

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
