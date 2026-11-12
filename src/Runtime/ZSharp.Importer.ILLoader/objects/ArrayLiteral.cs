namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// Values written as one, which become a CLR array where one is expected: an array
    /// of a type every value goes to, or an interface such an array implements, as
    /// <c>IEnumerable&lt;T&gt;</c>. With nothing expected it has no type, as a collection
    /// expression has none in C#.
    /// </summary>
    internal sealed class ArrayLiteral(ILLoader loader, CompilerObject[] items)
        : CompilerObject
        , ICTImplicitCastTo
        , ICTCastTo
    {
        public IResult ImplicitCast(CompilerObject type)
        {
            if (!type.Is<IILType>(out var known) || Element(known.GetILType()) is not { } element)
                return Result.Error(new UnsupportedError("Values in brackets only go to an array, or to what an array implements."));

            var values = new List<CompilerObject>();

            foreach (var item in items)
                if (ZSharp.Compiler.Current.CG.ImplicitCast(item, loader.Class(element)).Ok(out var value))
                    values.Add(value);
                else
                    return Result.Error(new UnsupportedError($"A value in brackets doesn't go to {element.Name}."));

            return Result.Ok(new Call(loader.ArrayOf(element, values.Count), [.. values], () => type));
        }

        public IResult Cast(CompilerObject type)
            => ImplicitCast(type);

        /// <summary>
        /// The element type of the array a type takes: its own if it is an array, or the
        /// one an array must have to implement it.
        /// </summary>
        private static Type? Element(Type type)
        {
            if (type.IsSZArray)
                return type.GetElementType();

            if (type.IsGenericType && type.GetGenericArguments() is [var argument] && argument.MakeArrayType().IsAssignableTo(type))
                return argument;

            return null;
        }
    }
}
