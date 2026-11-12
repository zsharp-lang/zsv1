using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// A local that a function captures. It lives in a cell, so the function and the code
    /// it belongs to share it. Declaring it makes the cell, with its initial value.
    /// </summary>
    internal sealed class CellLocal(CompilerObject type, CompilerObject? init)
        : CompilerObject
        , ICompileIRExpression
        , ICompileIRReference<LIR.Expression>
        , ICTSet
        , ICTDeclare
        , ITyped
    {
        private LIR.Local? cell;
        private LIR.Type? value;

        CompilerObject ITyped.Type => type;

        public IResult<LIR.Local, Error> Compile(object? target)
        {
            if (cell is not null)
                return Result<LIR.Local>.Ok(cell);

            if (Scope.IR.CompileType(type, target).When(out var compiled).Error(out var error))
                return Result<LIR.Local>.Error(error);

            value = compiled;

            return Result<LIR.Local>.Ok(cell = new(new LIR.Reference(compiled!)));
        }

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Compile(target).When(out var compiled).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Load(new LIR.GetLocal(compiled!)));

        /// <summary>
        /// The cell itself, which is what a function that captures the local is given.
        /// </summary>
        public IResult<LIR.Expression, Error> CompileIRReference(object? target)
            => Compile(target).When(out var compiled).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.GetLocal(compiled!));

        public IResult Set(CompilerObject value)
            => Scope.CG.ImplicitCast(value, type).Ok(out var converted)
                ? Result.Ok(new SetCell(this, converted))
                : Result.Error("The value doesn't match the type of this local.");

        public IResult Declare()
        {
            if (init is null)
                return Result.Ok(new DeclareCell(this, null));

            return Scope.CG.ImplicitCast(init, type).Ok(out var converted)
                ? Result.Ok(new DeclareCell(this, converted))
                : Result.Error("The initial value doesn't match the type of this local.");
        }

        private sealed class DeclareCell(CellLocal local, CompilerObject? initial)
            : CompilerObject
            , ICompileIRStatement
        {
            public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
            {
                if (local.Compile(target).When(out var cell).Error(out var error))
                    return Result<LIR.Statement>.Error(error);

                LIR.Expression value = new LIR.Constant(local.value!, null);

                if (initial is not null)
                    if (Scope.IR.CompileExpression(initial, target).When(out var compiled).Error(out error))
                        return Result<LIR.Statement>.Error(error);
                    else
                        value = compiled!;

                var block = new LIR.Block();

                block.Locals.Add(cell!);
                block.Statements.Add(new LIR.SetLocal(cell!, new LIR.Allocate(value)));

                return Result<LIR.Statement>.Ok(block);
            }
        }

        private sealed class SetCell(CellLocal local, CompilerObject value)
            : CompilerObject
            , ICompileIRExpression
            , ITyped
        {
            CompilerObject ITyped.Type => ((ITyped)local).Type;

            public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            {
                if (local.Compile(target).When(out var cell).Error(out var error))
                    return Result<LIR.Expression>.Error(error);

                if (Scope.IR.CompileExpression(value, target).When(out var result).Error(out error))
                    return Result<LIR.Expression>.Error(error);

                return Result<LIR.Expression>.Ok(new LIR.Sequence(
                    new LIR.Block(new LIR.Store(new LIR.GetLocal(cell!), result!)),
                    new LIR.Load(new LIR.GetLocal(cell!))
                ));
            }
        }
    }
}
