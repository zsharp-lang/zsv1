namespace ZSharp.Importer.RT
{
    partial class Loader
    {
        private Objects.StringLiteral Load(string value)
            => new(value, TypeSystem.String);
    }
}
