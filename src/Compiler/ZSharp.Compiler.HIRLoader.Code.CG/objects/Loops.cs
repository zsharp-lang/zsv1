using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// A loop, which a break or continue in it names. It has its LIR loop from the start,
    /// so they can refer to it before it is lowered. It lowers to
    /// <c>loop { if (condition) body else { else-part; break } }</c>: a continue checks
    /// the condition again, and a break skips the else part.
    /// </summary>
    internal abstract class Loop
        : CompilerObject
    {
        public LIR.Loop Lowered { get; } = new(new LIR.Block());

        public CompilerObject Condition { get; set; } = null!;

        public CompilerObject Body { get; set; } = null!;

        /// <summary>
        /// What leaving the loop with a value does before it breaks.
        /// </summary>
        public abstract IResult<LIR.Statement, Error> Leave(CompilerObject? value, object? target);

        protected IResult<LIR.Loop, Error> Lower(LIR.Statement otherwise, object? target)
        {
            if (Scope.IR.CompileExpression(Condition, target).When(out var test).Error(out var error))
                return Result<LIR.Loop>.Error(error);

            if (Scope.IR.CompileStatement(Body, target).When(out var body).Error(out error))
                return Result<LIR.Loop>.Error(error);

            Lowered.Body = new LIR.If(test!, body!, new LIR.Block(otherwise, new LIR.Break(Lowered)));

            return Result<LIR.Loop>.Ok(Lowered);
        }
    }

    internal sealed class WhileLoop
        : Loop
        , ICompileIRStatement
    {
        public CompilerObject? Else { get; set; }

        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
        {
            LIR.Statement otherwise = new LIR.Block();

            if (Else is not null && Scope.IR.CompileStatement(Else, target).When(out otherwise!).Error(out var error))
                return Result<LIR.Statement>.Error(error);

            return Lower(otherwise, target).When(out var loop).Error(out var failure)
                ? Result<LIR.Statement>.Error(failure)
                : Result<LIR.Statement>.Ok(loop!);
        }

        public override IResult<LIR.Statement, Error> Leave(CompilerObject? value, object? target)
            => Result<LIR.Statement>.Ok(new LIR.Break(Lowered));
    }

    /// <summary>
    /// A loop that is an expression. Its value is held in a local that a break or the
    /// else part sets before leaving it, and its type is that of the else part.
    /// </summary>
    internal sealed class WhileValue(CompilerObject type)
        : Loop
        , ICompileIRExpression
        , ITyped
    {
        private LIR.Local? result;

        CompilerObject ITyped.Type => type;

        public CompilerObject Else { get; set; } = null!;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (Result(target).When(out var local).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            if (Set(Else, target).When(out var otherwise).Error(out error))
                return Result<LIR.Expression>.Error(error);

            if (Lower(otherwise!, target).When(out var loop).Error(out error))
                return Result<LIR.Expression>.Error(error);

            var block = new LIR.Block(loop!);

            block.Locals.Add(local!);

            return Result<LIR.Expression>.Ok(new LIR.Sequence(block, new LIR.GetLocal(local!)));
        }

        public override IResult<LIR.Statement, Error> Leave(CompilerObject? value, object? target)
            => Set(value!, target).When(out var set).Error(out var error)
                ? Result<LIR.Statement>.Error(error)
                : Result<LIR.Statement>.Ok(new LIR.Block(set!, new LIR.Break(Lowered)));

        private IResult<LIR.Statement, Error> Set(CompilerObject value, object? target)
        {
            if (Result(target).When(out var local).Error(out var error))
                return Result<LIR.Statement>.Error(error);

            return Scope.IR.CompileExpression(value, target).When(out var compiled).Error(out error)
                ? Result<LIR.Statement>.Error(error)
                : Result<LIR.Statement>.Ok(new LIR.SetLocal(local!, compiled!));
        }

        private IResult<LIR.Local, Error> Result(object? target)
        {
            if (result is not null)
                return Result<LIR.Local>.Ok(result);

            return Scope.IR.CompileType(type, target).When(out var compiled).Error(out var error)
                ? Result<LIR.Local>.Error(error)
                : Result<LIR.Local>.Ok(result = new(compiled!) { Name = "loop" });
        }
    }

    internal sealed class Break(Loop loop, CompilerObject? value)
        : CompilerObject
        , ICompileIRStatement
    {
        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
            => loop.Leave(value, target);
    }

    internal sealed class Continue(Loop loop)
        : CompilerObject
        , ICompileIRStatement
    {
        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
            => Result<LIR.Statement>.Ok(new LIR.Continue(loop.Lowered));
    }
}
