using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// A parameter as a call matches arguments to it: it takes the argument with its name,
    /// the next positional one, or both in that order, as its kind allows, or else its
    /// default value, which is code loaded where the call is.
    /// </summary>
    internal sealed class ParameterMatch(
        string name,
        HIR.Type? type,
        HIR.Code.Expression? fallback,
        bool byName,
        bool byPosition
    )
        : CompilerObject
        , ICTMatchParameter
    {
        public IResult Match(ArgumentStream arguments)
        {
            if (type is null || !Scope.HIRLoader.LoadType(type).Ok(out var loaded))
                return Result.Error($"Parameter {name} has no type.");

            var argument = (byName ? arguments.Pop(name) : null) ?? (byPosition ? arguments.Pop() : null);

            if (argument is null && fallback is not null && Scope.HIRCodeLoader.LoadExpression(fallback).Ok(out var value))
                argument = value;

            return argument is null
                ? Result.Error($"No argument is given for parameter {name}.")
                : Scope.CG.ImplicitCast(argument, loaded);
        }
    }
}
