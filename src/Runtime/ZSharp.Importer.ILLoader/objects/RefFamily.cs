namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// <c>Ref</c> or <c>Out</c>, which gives the reference type of the type it is indexed
    /// with, as in <c>Ref[Int32]</c>.
    /// </summary>
    internal sealed class RefFamily(ILLoader loader, bool isOut)
        : CompilerObject
        , ICTGetIndex
    {
        public IResult Index(Argument[] arguments)
            => loader.Types(arguments, [typeof(Ref<>).GetGenericArguments()[0]]) is [var type] && !type.IsByRef
                ? Result.Ok(loader.Ref(type, isOut))
                : Result.Error($"{(isOut ? "Out" : "Ref")} takes one type, which isn't a reference.");
    }

    /// <summary>
    /// The shape of <c>Ref</c> and <c>Out</c> as an index takes arguments: one type, <c>T</c>.
    /// </summary>
    internal sealed class Ref<T>;
}
