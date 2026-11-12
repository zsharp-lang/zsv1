using Scope = ZSharp.Compiler.Current;

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
        , ICTImplicitCastTo
        , ICTCastTo
    {
        CompilerObject ITyped.Type => loader.Class(DelegateType() ?? typeof(Delegate));

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => DelegateType() is { } type
                ? Value(type, target)
                : Result<LIR.Expression>.Error($"Function {node.Name} can only be a value when its types are CLR types.");

        /// <summary>
        /// A local function goes where a delegate of a type is expected if its signature
        /// fits, as a method does.
        /// </summary>
        public IResult ImplicitCast(CompilerObject type)
            => type.Is<IILType>(out var known) && Signature() is var (parameters, returnType) && MethodValue.Fits(parameters, returnType, known.GetILType())
                ? Result.Ok(new AsDelegate(this, known.GetILType(), type))
                : Result.Error(new UnsupportedError($"{node.Name} isn't a delegate of that type."));

        public IResult Cast(CompilerObject type)
            => ImplicitCast(type);

        /// <summary>
        /// The function as a delegate of a type that fits it.
        /// </summary>
        private sealed class AsDelegate(LocalFunction function, Type type, CompilerObject delegateType)
            : CompilerObject
            , ICompileIRExpression
            , ITyped
        {
            CompilerObject ITyped.Type => delegateType;

            public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
                => function.Value(type, target);
        }

        private IResult<LIR.Expression, Error> Value(Type type, object? target)
        {
            if (loader.Lower(node).When(out var function).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            LIR.Expression value = new LIR.Call(
                loader.Handle(typeof(Closures).GetMethod(nameof(Closures.Bind))!),
                [new LIR.Constant(loader.Handle(typeof(object)), new LocalFunctionValue(loader.Runtime, function!, type))]
            );

            foreach (var capture in ILLoader.Passed(node))
            {
                if (Scope.HIRLoader.LoadInstance(capture).When(out var captured).Error(out error))
                    return Result<LIR.Expression>.Error(error);

                if (!ILLoader.InCell(captured!))
                    continue;

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
            => Signature() is var (parameters, returnType)
                ? System.Linq.Expressions.Expression.GetDelegateType([.. parameters, returnType])
                : null;

        /// <summary>
        /// The CLR types of the function's parameters and of what it returns, if its types
        /// are CLR types.
        /// </summary>
        private (Type[] Parameters, Type ReturnType)? Signature()
        {
            var types = new List<Type>();

            foreach (var parameter in Parameters() ?? [])
                if (parameter.Is<IILType>(out var known))
                    types.Add(known.GetILType());
                else
                    return null;

            if (node.ReturnType is null)
                return ([.. types], typeof(void));

            return Scope.HIRLoader.LoadType(node.ReturnType).Ok(out var returned) && returned.Is<IILType>(out var result)
                ? ([.. types], result.GetILType())
                : null;
        }

        public IResult Call(Argument[] arguments)
        {
            if (loader.Lower(node).When(out var function).Error(out var error))
                return Result.Error(error);

            if (Arguments.Parameters(node.Parameters) is not { } matching || Arguments.Match(matching, arguments) is not { } values)
                return Result.Error($"{node.Name} can't be called with these arguments.");

            var cells = new List<CompilerObject>();

            foreach (var capture in ILLoader.Passed(node))
                if (Scope.HIRLoader.LoadInstance(capture).When(out var captured).Error(out error))
                    return Result.Error(error);
                else if (ILLoader.InCell(captured!))
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
