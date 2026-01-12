namespace ZSharp.Importer.ILLoader
{
    internal interface ILoader<T>
    {
        public CompilerObject LoadMember(T member);
    }
}
