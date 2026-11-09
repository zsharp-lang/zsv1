using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// What a call calls, as each target has it: a definition is lowered for each target,
    /// so what calls one of its members finds the member as that target's lowering has it.
    /// </summary>
    internal delegate IResult<LIR.ICallable, Error> Callee(object? target);

    /// <summary>
    /// A call of a CLR function. A variable passed by reference that has no address of its
    /// own goes through a temporary, which the call assigns back once it returns.
    /// </summary>
    internal sealed class Call(
        LIR.ICallable callable,
        CompilerObject[] arguments,
        Func<CompilerObject>? returnType
    )
        : CompilerObject
        , ICompileIRExpression
        , ICompileIRStatement
        , ITyped
    {
        CompilerObject ITyped.Type => returnType!();

        // A call that returns nothing has no type.
        bool CompilerObject.Is<T>([NotNullWhen(true)] out T? result) where T : class
            => (result = returnType is null && typeof(T) == typeof(ITyped) ? null : this as T) is not null;

        private readonly Callee? callee;

        /// <summary>
        /// A call of what each target has, as a member of a definition.
        /// </summary>
        public Call(Callee callee, CompilerObject[] arguments, Func<CompilerObject>? returnType)
            : this((LIR.ICallable)null!, arguments, returnType)
            => this.callee = callee;

        private sealed record Lowered(LIR.Call Call, LIR.Block Before, List<LIR.Statement> After);

        private IResult<Lowered, Error> Lower(object? target)
        {
            var called = callable;

            if (callee is not null)
                if (callee(target).When(out var found).Error(out var missing))
                    return Result<Lowered>.Error(missing);
                else
                    called = found!;

            var values = new List<LIR.Expression>();
            var before = new LIR.Block();
            var after = new List<LIR.Statement>();

            foreach (var argument in arguments)
            {
                if (argument is ByAddress byAddress)
                {
                    if (byAddress.Lower(target, out var address, out var current) is { } failure)
                        return Result<Lowered>.Error(failure);

                    if (address is not null)
                    {
                        values.Add(address);
                        continue;
                    }

                    var temporary = new LIR.Local(byAddress.ElementType);

                    before.Locals.Add(temporary);
                    before.Statements.Add(new LIR.SetLocal(temporary, current!));
                    values.Add(new LIR.CLR.AddressOf(new LIR.GetLocal(temporary), byAddress.AddressType));

                    if (Core.Compiler.ModuleScope.CG.Set(byAddress.Variable, new Temporary(temporary, byAddress.ElementClass)).When(out var assign).Error(out failure))
                        return Result<Lowered>.Error(failure);

                    if (Core.Compiler.ModuleScope.IR.CompileStatement(assign!, target).When(out var assigned).Error(out failure))
                        return Result<Lowered>.Error(failure);

                    after.Add(assigned!);
                    continue;
                }

                if (Core.Compiler.ModuleScope.IR.CompileExpression(argument, target).When(out var value).Error(out var error))
                    return Result<Lowered>.Error(error);

                values.Add(value!);
            }

            return Result<Lowered>.Ok(new(new LIR.Call(called, values), before, after));
        }

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (Lower(target).When(out var lowered).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            if (lowered!.After.Count == 0)
                return Result<LIR.Expression>.Ok(lowered.Call);

            if (returnType is null)
                return Result<LIR.Expression>.Error("A call that returns nothing has no value.");

            if (Core.Compiler.ModuleScope.IR.CompileType(returnType(), target).When(out var type).Error(out error))
                return Result<LIR.Expression>.Error(error);

            var result = new LIR.Local(type!);
            var block = lowered.Before;

            block.Locals.Add(result);
            block.Statements.Add(new LIR.SetLocal(result, lowered.Call));
            block.Statements.AddRange(lowered.After);

            return Result<LIR.Expression>.Ok(new LIR.Sequence(block, new LIR.GetLocal(result)));
        }

        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
        {
            if (Lower(target).When(out var lowered).Error(out var error))
                return Result<LIR.Statement>.Error(error);

            if (lowered!.After.Count == 0)
                return Result<LIR.Statement>.Ok(new LIR.Evaluate(lowered.Call));

            var block = lowered.Before;

            block.Statements.Add(new LIR.Evaluate(lowered.Call));
            block.Statements.AddRange(lowered.After);

            return Result<LIR.Statement>.Ok(block);
        }
    }
}
