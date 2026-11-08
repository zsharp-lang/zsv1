using System.Reflection;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Infers the type arguments of a generic method from the types of a call's
    /// arguments, as C# does from what it can see: each parameter's type is matched with
    /// its argument's, through arrays, references and the constructed types an argument's
    /// type is or derives from. An argument with no type says nothing.
    /// </summary>
    internal static class Inference
    {
        public static Type[]? Infer(MethodInfo generic, Argument[] arguments)
        {
            var parameters = generic.GetParameters();
            var positional = arguments.Where(argument => argument.Name is null).Select(argument => argument.Object).ToList();
            var bound = new Dictionary<Type, Type>();
            var next = 0;

            foreach (var (parameter, index) in parameters.Select((parameter, index) => (parameter, index)))
            {
                if (arguments.FirstOrDefault(argument => argument.Name is not null && argument.Name == parameter.Name) is { } named)
                {
                    Unify(parameter.ParameterType, TypeOf(named.Object), bound);
                    continue;
                }

                if (index == parameters.Length - 1 && parameter.IsDefined(typeof(ParamArrayAttribute)))
                {
                    var rest = positional.Skip(next).ToList();

                    if (rest is [var single] && TypeOf(single) is { IsArray: true } array)
                        Unify(parameter.ParameterType, array, bound);
                    else
                        foreach (var item in rest)
                            Unify(parameter.ParameterType.GetElementType()!, TypeOf(item), bound);

                    break;
                }

                if (next < positional.Count)
                    Unify(parameter.ParameterType, TypeOf(positional[next++]), bound);
            }

            var types = generic.GetGenericArguments();

            return types.All(bound.ContainsKey) ? [.. types.Select(type => bound[type])] : null;
        }

        /// <summary>
        /// Whether a parameter could take a value of a type: a type that isn't generic takes
        /// what goes to it, and a generic one takes what it can be inferred from.
        /// </summary>
        public static bool Accepts(Type parameter, Type argument)
        {
            if (!parameter.ContainsGenericParameters)
                return parameter.IsAssignableFrom(argument);

            if (parameter.IsGenericMethodParameter)
                return true;

            if (parameter.IsArray)
                return argument.IsArray;

            return parameter.IsGenericType
                && Hierarchy(argument).Any(type => type.IsGenericType && type.GetGenericTypeDefinition() == parameter.GetGenericTypeDefinition());
        }

        private static Type? TypeOf(CompilerObject argument)
            => Core.Compiler.ModuleScope.TS.TypeOf(argument).Ok(out var type) && type.Is<IILType>(out var known)
                ? known.GetILType()
                : null;

        private static void Unify(Type parameter, Type? argument, Dictionary<Type, Type> bound)
        {
            if (argument is null)
                return;

            if (parameter.IsByRef)
                parameter = parameter.GetElementType()!;

            if (argument.IsByRef)
                argument = argument.GetElementType()!;

            if (parameter.IsGenericMethodParameter)
            {
                // Two arguments for one type parameter give the more general of the two, if
                // one of them is.
                if (!bound.TryGetValue(parameter, out var known) || (!known.IsAssignableFrom(argument) && argument.IsAssignableFrom(known)))
                    bound[parameter] = argument;

                return;
            }

            if (!parameter.ContainsGenericParameters)
                return;

            if (parameter.IsArray)
            {
                if (argument.IsArray && argument.GetArrayRank() == parameter.GetArrayRank())
                    Unify(parameter.GetElementType()!, argument.GetElementType(), bound);

                return;
            }

            if (!parameter.IsGenericType)
                return;

            var definition = parameter.GetGenericTypeDefinition();

            if (Hierarchy(argument).FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == definition) is not { } match)
                return;

            foreach (var (inner, given) in parameter.GetGenericArguments().Zip(match.GetGenericArguments()))
                Unify(inner, given, bound);
        }

        private static IEnumerable<Type> Hierarchy(Type type)
        {
            for (var current = type; current is not null; current = current.BaseType)
                yield return current;

            foreach (var @interface in type.GetInterfaces())
                yield return @interface;
        }
    }
}
