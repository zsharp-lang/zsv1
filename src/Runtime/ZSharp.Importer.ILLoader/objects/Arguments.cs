namespace ZSharp.Importer.ILLoader.Objects
{
    internal static class Arguments
    {
        /// <summary>
        /// Converts arguments to the types of the parameters they go to, or returns
        /// <see langword="null"/> if one of them can't be.
        /// </summary>
        public static CompilerObject[]? Convert(IEnumerable<CompilerObject> types, Argument[] arguments)
        {
            var converted = new List<CompilerObject>();

            foreach (var (type, argument) in types.Zip(arguments))
                if (Core.Compiler.ModuleScope.CG.ImplicitCast(argument.Object, type).Ok(out var value))
                    converted.Add(value);
                else
                    return null;

            return [.. converted];
        }

        /// <summary>
        /// Converts arguments to parameters of something being defined.
        /// </summary>
        public static CompilerObject[]? Convert(List<Package.DotNet.Parameter> parameters, Argument[] arguments)
        {
            if (parameters.Count != arguments.Length)
                return null;

            var types = new List<CompilerObject>();

            foreach (var parameter in parameters)
                if (parameter.Type is not null && Core.Compiler.ModuleScope.HIRLoader.LoadType(parameter.Type).Ok(out var type))
                    types.Add(type);
                else
                    return null;

            return Convert(types, arguments);
        }
    }
}
