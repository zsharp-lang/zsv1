using System.Reflection;

using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal static class Arguments
    {
        /// <summary>
        /// The values of parameters for a call. Each parameter, in order, takes what it
        /// needs from the call's arguments, and any argument left over is one too many.
        /// Returns <see langword="null"/> if they don't match.
        /// </summary>
        public static CompilerObject[]? Match(IEnumerable<CompilerObject> parameters, Argument[] arguments)
        {
            var stream = new ArgumentStream(arguments);
            var values = new List<CompilerObject>();

            foreach (var parameter in parameters)
                if (Scope.Matching.Bind(parameter, stream).Ok(out var value))
                    values.Add(value);
                else
                    return null;

            return stream.HasArguments ? null : [.. values];
        }

        /// <summary>
        /// What an ordinary parameter takes: the argument with its name, or else the next
        /// positional one, or else its default value, converted to its type.
        /// </summary>
        public static IResult Take(string name, CompilerObject type, Func<CompilerObject?>? fallback, ArgumentStream arguments)
        {
            if ((arguments.Pop(name) ?? arguments.Pop() ?? fallback?.Invoke()) is not { } argument)
                return Result.Error($"No argument is given for parameter {name}.");

            return Scope.CG.ImplicitCast(argument, type);
        }

        /// <summary>
        /// The parameters of something being defined, as they match arguments.
        /// </summary>
        public static CompilerObject[]? Parameters(List<HIR.Code.Parameter> parameters)
        {
            var loaded = new List<CompilerObject>();

            foreach (var parameter in parameters)
                if (Scope.HIRLoader.LoadDefinition(parameter).Ok(out var definition))
                    loaded.Add(definition);
                else
                    return null;

            return [.. loaded];
        }

        /// <summary>
        /// The types of the parameters of something being defined, or <see langword="null"/>
        /// if one of them can't be loaded.
        /// </summary>
        public static CompilerObject[]? Types(List<HIR.Code.Parameter> parameters)
        {
            var types = new List<CompilerObject>();

            foreach (var parameter in parameters)
                if (parameter.Type is not null && Scope.HIRLoader.LoadType(parameter.Type).Ok(out var type))
                    types.Add(type);
                else
                    return null;

            return [.. types];
        }
    }

    /// <summary>
    /// A parameter of a CLR method, with the default value its metadata gives. A
    /// <c>params</c> array takes the rest of the positional arguments, as in C#: one that is
    /// already an array of its type is the array, and otherwise each is an element.
    /// </summary>
    internal sealed class ClrParameter(ParameterInfo parameter, ILLoader loader)
        : CompilerObject
        , ICTBind
    {
        public IResult Bind(ArgumentStream arguments)
        {
            var name = parameter.Name ?? string.Empty;
            var type = loader.ParameterType(parameter);

            if (!parameter.IsDefined(typeof(ParamArrayAttribute)))
                return Arguments.Take(
                    name,
                    type,
                    parameter.HasDefaultValue ? () => new Literal(parameter.DefaultValue, parameter.ParameterType, loader) : null,
                    arguments
                );

            // Given by its name, it is the array itself.
            if (arguments.Pop(name) is { } named)
                return Scope.CG.ImplicitCast(named, type);

            var given = new List<CompilerObject>();

            while (arguments.Pop() is { } argument)
                given.Add(argument);

            if (given is [var single] && Scope.CG.ImplicitCast(single, type).Ok(out var array))
                return Result.Ok(array);

            var element = parameter.ParameterType.GetElementType()!;
            var elements = new List<CompilerObject>();

            foreach (var argument in given)
                if (Scope.CG.ImplicitCast(argument, loader.Class(element)).Ok(out var converted))
                    elements.Add(converted);
                else
                    return Result.Error($"An argument doesn't match the elements of {name}.");

            return Result.Ok(new Call(loader.ArrayOf(element, elements.Count), [.. elements], () => type));
        }
    }

    /// <summary>
    /// A parameter defined in HIR, as a call matches arguments to it. Its default value is
    /// code, loaded where the call is. The code it belongs to uses it as a variable, which
    /// is what it is as an instance.
    /// </summary>
    internal sealed class DefinedParameter(Package.DotNet.Parameter node)
        : CompilerObject
        , ICTBind
    {
        public IResult Bind(ArgumentStream arguments)
        {
            if (node.Type is null || !Scope.HIRLoader.LoadType(node.Type).Ok(out var type))
                return Result.Error($"Parameter {node.Name} has no type.");

            return Arguments.Take(
                node.Name,
                type,
                node.DefaultValue is { } fallback
                    ? () => Scope.HIRCodeLoader.LoadExpression(fallback).Ok(out var loaded) ? loaded : null
                    : null,
                arguments
            );
        }
    }
}
