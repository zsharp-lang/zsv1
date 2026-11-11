using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// An operator lifted to nullable operands, as C# lifts it. It applies only when an
    /// operand is nullable, and then calls the operator on the operands' values if they
    /// all have one. A <c>null</c> operand goes to the type of the other.
    /// </summary>
    internal sealed class LiftedOperator(ILLoader loader, HIR.Code.OverloadGroup @operator, Package.DotNet.Lifting lifting)
        : CompilerObject
        , ICTCallable
    {
        public IResult Call(Argument[] arguments)
        {
            if (arguments is not [{ Name: null } left, { Name: null } right])
                return Result.Error(new UnsupportedError("A lifted operator takes two operands."));

            var operands = new[] { left.Object, right.Object };
            var types = operands.Select(TypeOf).ToArray();

            if (!types.Any(type => type is not null && Nullable.GetUnderlyingType(type) is not null))
                return Result.Error(new UnsupportedError("No operand is nullable."));

            // An operand with no type of its own, as null, goes to the type of the other.
            for (var index = 0; index < 2; index++)
                if (types[index] is null && types[1 - index] is { } other)
                {
                    var nullable = Nullable.GetUnderlyingType(other) is null && other.IsValueType ? typeof(Nullable<>).MakeGenericType(other) : other;

                    if (!Scope.CG.ImplicitCast(operands[index], loader.Class(nullable)).Ok(out var converted))
                        return Result.Error(new UnsupportedError("An operand doesn't go to the type of the other."));

                    (operands[index], types[index]) = (converted, nullable);
                }

            if (types.Any(type => type is null))
                return Result.Error(new UnsupportedError("An operand has no type."));

            var temporaries = new List<(LIR.Local Local, CompilerObject Operand)>();
            var values = new List<Argument>();
            var hasValues = new List<CompilerObject>();
            var present = new List<CompilerObject>();

            foreach (var (operand, operandType) in operands.Zip(types))
            {
                var local = new LIR.Local(loader.Handle(operandType!));
                var temporary = new Temporary(local, loader.Class(operandType!));

                temporaries.Add((local, operand));

                if (Nullable.GetUnderlyingType(operandType!) is null)
                {
                    values.Add(new(temporary));
                    present.Add(new Literal(true, typeof(bool), loader));
                    continue;
                }

                if (Scope.CG.Member(temporary, "HasValue").When(out var hasValue).Error(out var error))
                    return Result.Error(error);

                if (Scope.CG.Member(temporary, "GetValueOrDefault").When(out var getValue).Error(out error)
                    || Scope.CG.Call(getValue!, []).When(out var value).Error(out error))
                    return Result.Error(error);

                values.Add(new(value!));
                hasValues.Add(hasValue!);
                present.Add(hasValue!);
            }

            if (Scope.HIRLoader.LoadInstance(@operator).When(out var function).Error(out var failure))
                return Result.Error(failure);

            if (Scope.CG.Call(function!, [.. values]).When(out var applied).Error(out failure))
                return Result.Error(failure);

            if (TypeOf(applied!) is not { } result)
                return Result.Error(new UnsupportedError("The operator gives nothing to lift."));

            CompilerObject then = applied!, otherwise;
            Type type;

            switch (lifting)
            {
                case Package.DotNet.Lifting.Value:
                    type = result.IsValueType && Nullable.GetUnderlyingType(result) is null ? typeof(Nullable<>).MakeGenericType(result) : result;

                    if (!Scope.CG.ImplicitCast(applied!, loader.Class(type)).Ok(out then))
                        return Result.Error(new UnsupportedError($"{result.Name} can't be lifted."));

                    otherwise = new Literal(null, type, loader);
                    break;
                case Package.DotNet.Lifting.Equality:
                    type = result;

                    if (Scope.CG.Call(function!, [.. present.Select(value => new Argument(value))]).When(out otherwise).Error(out failure))
                        return Result.Error(failure);
                    break;
                default:
                    type = result;
                    otherwise = new Literal(false, typeof(bool), loader);
                    break;
            }

            return Result.Ok(new LiftedCall(loader, [.. temporaries], [.. hasValues], then, otherwise!, type));
        }

        private static Type? TypeOf(CompilerObject value)
            => Scope.TS.TypeOf(value).Ok(out var type) && type.Is<IILType>(out var known) ? known.GetILType() : null;
    }

    /// <summary>
    /// A lifted operator's code: the operands kept, then the operator applied if they all
    /// have a value, and what it gives otherwise if not.
    /// </summary>
    internal sealed class LiftedCall(
        ILLoader loader,
        (LIR.Local Local, CompilerObject Operand)[] temporaries,
        CompilerObject[] hasValues,
        CompilerObject then,
        CompilerObject otherwise,
        Type type
    )
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => loader.Class(type);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            var block = new LIR.Block();
            var result = new LIR.Local(loader.Handle(type));

            foreach (var (local, operand) in temporaries)
                if (Scope.IR.CompileExpression(operand, target).When(out var value).Error(out var error))
                    return Result<LIR.Expression>.Error(error);
                else
                {
                    block.Locals.Add(local);
                    block.Statements.Add(new LIR.SetLocal(local, value!));
                }

            block.Locals.Add(result);

            if (Assign(result, then, target).When(out var statement).Error(out var failure))
                return Result<LIR.Expression>.Error(failure);

            foreach (var hasValue in hasValues.Reverse())
            {
                if (Scope.IR.CompileExpression(hasValue, target).When(out var test).Error(out failure))
                    return Result<LIR.Expression>.Error(failure);

                if (Assign(result, otherwise, target).When(out var no).Error(out failure))
                    return Result<LIR.Expression>.Error(failure);

                statement = new LIR.If(test!, statement!, no);
            }

            block.Statements.Add(statement!);

            return Result<LIR.Expression>.Ok(new LIR.Sequence(block, new LIR.GetLocal(result)));
        }

        private static IResult<LIR.Statement, Error> Assign(LIR.Local local, CompilerObject value, object? target)
            => Scope.IR.CompileExpression(value, target).When(out var compiled).Error(out var error)
                ? Result<LIR.Statement>.Error(error)
                : Result<LIR.Statement>.Ok(new LIR.SetLocal(local, compiled!));
    }
}
