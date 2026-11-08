using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Compiler.Objects
{
    internal sealed class Block(CompilerObject[] statements)
        : CompilerObject
        , ICompileIRStatement
    {
        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
            => Compile(target).When(out var block).Error(out var error)
                ? Result<LIR.Statement>.Error(error)
                : Result<LIR.Statement>.Ok(block!);

        public IResult<LIR.Block, Error> Compile(object? target)
        {
            var block = new LIR.Block();

            foreach (var statement in statements)
                if (Scope.IR.CompileStatement(statement, target).When(out var compiled).Error(out var error))
                    return Result<LIR.Block>.Error(error);
                else
                    block.Statements.Add(compiled!);

            return Result<LIR.Block>.Ok(block);
        }
    }

    /// <summary>
    /// Statements, then a value. A block that ends in a variable, as a <c>let</c> does, is
    /// that variable once its statements have run: assigning it assigns the variable, so
    /// it can be passed by reference, as in <c>Int32.TryParse(text, let n: Int32)</c>.
    /// </summary>
    internal sealed class BlockExpression(Block block, CompilerObject result)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
        , ICTSet
    {
        CompilerObject ITyped.Type => Scope.TS.TypeOf(result).Unwrap();

        // A block can be assigned only if what it ends in can.
        bool CompilerObject.Is<T>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? found) where T : class
            => (found = typeof(T) == typeof(ICTSet) && !result.Is<ICTSet>(out _) ? null : this as T) is not null;

        public IResult Set(CompilerObject value)
            => Scope.CG.Set(result, value);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (block.Compile(target).When(out var statements).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            return Scope.IR.CompileExpression(result, target).When(out var value).Error(out error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Sequence(statements!, value!));
        }
    }

    internal sealed class If(CompilerObject condition, CompilerObject then, CompilerObject? otherwise)
        : CompilerObject
        , ICompileIRStatement
    {
        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
        {
            if (Scope.IR.CompileExpression(condition, target).When(out var test).Error(out var error))
                return Result<LIR.Statement>.Error(error);

            if (Scope.IR.CompileStatement(then, target).When(out var yes).Error(out error))
                return Result<LIR.Statement>.Error(error);

            LIR.Statement? no = null;

            if (otherwise is not null && Scope.IR.CompileStatement(otherwise, target).When(out no).Error(out error))
                return Result<LIR.Statement>.Error(error);

            return Result<LIR.Statement>.Ok(new LIR.If(test!, yes!, no));
        }
    }

    internal sealed class Return(CompilerObject? value)
        : CompilerObject
        , ICompileIRStatement
    {
        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
        {
            if (value is null)
                return Result<LIR.Statement>.Ok(new LIR.Return());

            return Scope.IR.CompileExpression(value, target).When(out var result).Error(out var error)
                ? Result<LIR.Statement>.Error(error)
                : Result<LIR.Statement>.Ok(new LIR.Return(result));
        }
    }
}
