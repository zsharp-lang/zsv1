using ZSharp.HIR;

namespace ZSharp.SourceCompiler
{
    internal sealed class COStringImporterAdapter(ICOStringImporter importer)
        : IStringImporter
    {
        IResult<Expression, Error> IStringImporter.Import(string source)
            => importer.Import(source).When(co => new COImportResult(co));
    }
}
