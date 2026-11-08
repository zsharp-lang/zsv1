using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A function defined in the code of a method. It is a function of its own, which
    /// takes the cell of each variable it captures before its own parameters. Calling it
    /// passes the cells as the caller has them. As a value, it is a delegate bound to
    /// those cells.
    /// </summary>
    internal sealed class LocalFunction(ILLoader loader, Package.DotNet.LocalFunction node)
        : CompilerObject
        , ICTCallable
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => loader.Class(DelegateType() ?? typeof(Delegate));

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (loader.Lower(node).When(out var function).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            if (DelegateType() is not { } type)
                return Result<LIR.Expression>.Error($"Function {node.Name} can only be a value when its types are CLR types.");

            LIR.Expression value = new LIR.Call(
                loader.Handle(typeof(Closures).GetMethod(nameof(Closures.Bind))!),
                [new LIR.Constant(loader.Handle(typeof(object)), new LocalFunctionValue(loader.Runtime, function!, type))]
            );

            foreach (var capture in node.Captures)
            {
                if (Scope.HIRLoader.LoadInstance(capture).When(out var captured).Error(out error))
                    return Result<LIR.Expression>.Error(error);

                if (Scope.IR.CompileReference<LIR.Expression>(captured!, target).When(out var cell).Error(out error))
                    return Result<LIR.Expression>.Error(error);

                value = new LIR.Call(loader.Handle(typeof(Closures).GetMethod(nameof(Closures.With))!), [value, cell!]);
            }

            return Result<LIR.Expression>.Ok(new LIR.Call(loader.Handle(typeof(Closures).GetMethod(nameof(Closures.Make))!), [value]));
        }

        /// <summary>
        /// The delegate type of the function's values, if its types are CLR types.
        /// </summary>
        private Type? DelegateType()
        {
            var types = new List<Type>();

            foreach (var parameter in Parameters() ?? [])
                if (parameter.Is<IILType>(out var known))
                    types.Add(known.GetILType());
                else
                    return null;

            if (node.ReturnType is null)
                types.Add(typeof(void));
            else if (Scope.HIRLoader.LoadType(node.ReturnType).Ok(out var returned) && returned.Is<IILType>(out var known))
                types.Add(known.GetILType());
            else
                return null;

            return System.Linq.Expressions.Expression.GetDelegateType([.. types]);
        }

        public IResult Call(Argument[] arguments)
        {
            if (loader.Lower(node).When(out var function).Error(out var error))
                return Result.Error(error);

            if (Arguments.Parameters(node.Parameters) is not { } matching || Arguments.Match(matching, arguments) is not { } values)
                return Result.Error($"{node.Name} can't be called with these arguments.");

            var cells = new List<CompilerObject>();

            foreach (var capture in node.Captures)
                if (Scope.HIRLoader.LoadInstance(capture).When(out var captured).Error(out error))
                    return Result.Error(error);
                else
                    cells.Add(new CellOf(captured!));

            return Result.Ok(new Call(
                function!,
                [.. cells, .. values],
                node.ReturnType is { } returned ? () => Scope.HIRLoader.LoadType(returned).Unwrap() : null
            ));
        }

        private CompilerObject[]? Parameters()
        {
            var types = new List<CompilerObject>();

            foreach (var parameter in node.Parameters)
                if (parameter.Type is not null && Scope.HIRLoader.LoadType(parameter.Type).Ok(out var type))
                    types.Add(type);
                else
                    return null;

            return [.. types];
        }

        /// <summary>
        /// The cell a captured variable lives in, rather than its value.
        /// </summary>
        private sealed class CellOf(CompilerObject variable)
            : CompilerObject
            , ICompileIRExpression
        {
            public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
                => Scope.IR.CompileReference<LIR.Expression>(variable, target);
        }
    }

    /// <summary>
    /// A captured variable as a cell: inside a function that captures it, the cell it was
    /// given, and in the code a captured parameter belongs to, the cell it was copied into.
    /// </summary>
    internal sealed class Cell(LIR.Variable cell, CompilerObject type)
        : CompilerObject
        , ICompileIRExpression
        , ICompileIRReference<LIR.Expression>
        , ICTSet
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.Load(new LIR.GetLocal(cell)));

        public IResult<LIR.Expression, Error> CompileIRReference(object? target)
            => Result<LIR.Expression>.Ok(new LIR.GetLocal(cell));

        public IResult Set(CompilerObject value)
            => Scope.CG.ImplicitCast(value, type).Ok(out var converted)
                ? Result.Ok(new SetCell(cell, type, converted))
                : Result.Error("The value doesn't match the type of this variable.");

        private sealed class SetCell(LIR.Variable cell, CompilerObject type, CompilerObject value)
            : CompilerObject
            , ICompileIRExpression
            , ITyped
        {
            CompilerObject ITyped.Type => type;

            public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
                => Scope.IR.CompileExpression(value, target).When(out var result).Error(out var error)
                    ? Result<LIR.Expression>.Error(error)
                    : Result<LIR.Expression>.Ok(new LIR.Sequence(
                        new LIR.Block(new LIR.Store(new LIR.GetLocal(cell), result!)),
                        new LIR.Load(new LIR.GetLocal(cell))
                    ));
        }
    }
}
