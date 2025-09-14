namespace ZSharp.Importer.ILLoader
{
    public interface ITypeModifier
    {
        public CompilerObject Modify(CompilerObject type);
    }
}
