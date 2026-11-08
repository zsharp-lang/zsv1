using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A function of a module: a function of its own, called with its arguments. Once its
    /// module is loaded it is a CLR method, so it is a delegate where one is expected, as
    /// any CLR method is.
    /// </summary>
    internal sealed class ModuleFunction(ILLoader loader, Package.DotNet.Method node, LIR.Function function)
        : CompilerObject
        , ICTCallable
        , ICTImplicitCastTo
        , ICTCastTo
    {
        public IResult ImplicitCast(CompilerObject type)
            => Loaded() is { } method
                ? new Method(loader, method, null).ImplicitCast(type)
                : Result.Error(new UnsupportedError($"{node.Name} is a delegate only once its module is loaded."));

        public IResult Cast(CompilerObject type)
            => ImplicitCast(type);

        private System.Reflection.MethodInfo? Loaded()
        {
            try
            {
                return loader.Runtime.GetMethod(function);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public IResult Call(Argument[] arguments)
        {
            if (Arguments.Parameters(node.Parameters) is not { } matching || Arguments.Match(matching, arguments) is not { } values)
                return Result.Error($"{node.Name} can't be called with these arguments.");

            return Result.Ok(new Call(
                function,
                values,
                node.ReturnType is { } returned ? () => Scope.HIRLoader.LoadType(returned).Unwrap() : null
            ));
        }
    }

    /// <summary>
    /// A <c>let</c> of a module: a global of its LIR module.
    /// </summary>
    internal sealed class ModuleGlobal(LIR.Global global, Func<CompilerObject> type)
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
    {
        CompilerObject ITyped.Type => type();

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.GetGlobal(global));

        public IResult Set(CompilerObject value)
            => Scope.CG.ImplicitCast(value, type()).Ok(out var converted)
                ? Result.Ok(new SetGlobal(global, converted, type))
                : Result.Error("The value doesn't match the type of this variable.");

        private sealed class SetGlobal(LIR.Global global, CompilerObject value, Func<CompilerObject> type)
            : CompilerObject
            , ICompileIRExpression
            , ITyped
        {
            CompilerObject ITyped.Type => type();

            public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
                => Scope.IR.CompileExpression(value, target).When(out var result).Error(out var error)
                    ? Result<LIR.Expression>.Error(error)
                    : Result<LIR.Expression>.Ok(new LIR.Sequence(
                        new LIR.Block(new LIR.SetGlobal(global, result!)),
                        new LIR.GetGlobal(global)
                    ));
        }
    }
}
