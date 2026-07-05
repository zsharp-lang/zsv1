namespace ZSharp.SourceCompiler
{
    public interface IStringImporter
    {
        public IResult<HIR.Expression, Error> Import(string source);
    }
}
