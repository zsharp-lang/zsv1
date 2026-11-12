using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// A parameter as a call matches arguments to it. It takes the argument with its name,
    /// the next positional one, or both in that order, as its kind allows; or else its
    /// default value. It knows nothing of HIR: what loads it gives its type and its
    /// default value, which it asks for at each match. A type is an expression, which
    /// means what its context makes it, as a type parameter does for each instance of a
    /// generic definition; and the default is code that runs where the call is.
    /// </summary>
    internal sealed class ParameterMatch(
        string name,
        Func<IResult> type,
        Func<IResult>? fallback,
        bool byName,
        bool byPosition
    )
        : CompilerObject
        , ICTBind
    {
        public IResult Bind(ArgumentStream arguments)
        {
            if (type().When(out var parameterType).Error(out var error))
                return Result.Error(error);

            var argument = (byName ? arguments.Pop(name) : null) ?? (byPosition ? arguments.Pop() : null);

            if (argument is null && fallback is not null)
            {
                if (fallback().When(out var value).Error(out error))
                    return Result.Error(error);

                argument = value;
            }

            return argument is null
                ? Result.Error($"No argument is given for parameter {name}.")
                : Scope.CG.ImplicitCast(argument, parameterType!);
        }
    }
}
