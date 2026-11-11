namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A comparison C# predefines for CLR types that define no operator of their own:
    /// enums compare by their values, and references are equal if they are one object.
    /// It applies to two arguments of its type, and a reference comparison only to
    /// references, or <c>null</c>.
    /// </summary>
    internal sealed class Comparison(ILLoader loader, string name, Type operand)
        : CompilerObject
        , ICTCallable
    {
        public IResult Call(Argument[] arguments)
        {
            if (arguments is not [{ Name: null } left, { Name: null } right])
                return Result.Error(new UnsupportedError($"{name} compares two values."));

            var converted = new List<CompilerObject>();

            foreach (var argument in new[] { left, right })
            {
                // A value of a value type is no reference, though it would box to one.
                if (Core.Compiler.ModuleScope.TS.TypeOf(argument.Object).Ok(out var type)
                    && type.Is<IILType>(out var known) && known.GetILType().IsValueType && !operand.IsValueType)
                    return Result.Error(new UnsupportedError("A value type is compared by what its type defines."));

                if (Core.Compiler.ModuleScope.CG.ImplicitCast(argument.Object, loader.Class(operand)).Ok(out var value))
                    converted.Add(value);
                else
                    return Result.Error(new UnsupportedError($"{name} of {operand.Name} doesn't apply to these arguments."));
            }

            return Result.Ok(new Call(loader.Comparison(name, operand), [.. converted], () => loader.Class(typeof(bool))));
        }
    }
}
