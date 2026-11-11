using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// The operators the CLR types of the operands define, found when called: the static
    /// methods of those types that have the operator's name, chosen between as overloads,
    /// and the comparisons C# predefines, of enums and of references, which those methods
    /// are more specific than.
    /// </summary>
    internal sealed class ClrOperator(ILLoader loader, string name)
        : CompilerObject
        , ICTCallable
    {
        private static readonly HashSet<string> comparisons =
            ["op_Equality", "op_Inequality", "op_LessThan", "op_GreaterThan", "op_LessThanOrEqual", "op_GreaterThanOrEqual"];

        public IResult Call(Argument[] arguments)
        {
            var types = arguments
                .Select(argument => ZSharp.Compiler.Current.TS.TypeOf(argument.Object).Ok(out var type) && type.Is<IILType>(out var known) ? known.GetILType() : null)
                .OfType<Type>()
                .Distinct()
                .ToArray();

            var candidates = types
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(method => method.Name == name))
                .Distinct()
                .Select(method => (CompilerObject)new Method(loader, method, null))
                .ToList();

            if (comparisons.Contains(name))
                candidates.AddRange(types.Where(type => type.IsEnum).Select(type => new Comparison(loader, name, type)));

            if (name is "op_Equality" or "op_Inequality")
                candidates.Add(new Comparison(loader, name, typeof(object)));

            return candidates.Count == 0
                ? Result.Error(new UnsupportedError($"The operands define no {name}."))
                : ZSharp.Compiler.Current.CG.Call(OverloadGroup.Of(candidates), arguments);
        }
    }
}
