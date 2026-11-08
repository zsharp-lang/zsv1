namespace ZSharp.Importer.ILLoader.Objects
{
    internal static class Arguments
    {
        /// <summary>
        /// Converts arguments to the types of the parameters they go to, or returns
        /// <see langword="null"/> if there are more or fewer of them, or one can't be.
        /// </summary>
        public static CompilerObject[]? Convert(CompilerObject[] types, Argument[] arguments)
        {
            if (types.Length != arguments.Length)
                return null;

            var converted = new List<CompilerObject>();

            foreach (var (type, argument) in types.Zip(arguments))
                if (Core.Compiler.ModuleScope.CG.ImplicitCast(argument.Object, type).Ok(out var value))
                    converted.Add(value);
                else
                    return null;

            return [.. converted];
        }

        /// <summary>
        /// The types of the parameters of something being defined, or <see langword="null"/>
        /// if one of them can't be loaded.
        /// </summary>
        public static CompilerObject[]? Types(List<Package.DotNet.Parameter> parameters)
        {
            var types = new List<CompilerObject>();

            foreach (var parameter in parameters)
                if (parameter.Type is not null && Core.Compiler.ModuleScope.HIRLoader.LoadType(parameter.Type).Ok(out var type))
                    types.Add(type);
                else
                    return null;

            return [.. types];
        }
    }
}
