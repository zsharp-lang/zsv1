namespace ZSharp.Importer.String
{
    /// <summary>
    /// A function that imports a source, as an importer, as the package manager's is.
    /// </summary>
    public sealed class FunctionImporter(Func<string, IResult<HIR.Instance, Error>> import)
        : IStringImporter
    {
        public IResult<HIR.Instance, Error> Import(string source) => import(source);
    }
}
