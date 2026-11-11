global using CommonZ;

namespace ZSharp.Importer.String
{
    public interface IStringImporter
    {
        public IResult<HIR.Instance, Error> Import(string source);
    }
}
