namespace ZSharp.Importer.ILLoader
{
    public static class ILLoaderExtensions
    {
        public static CompilerObject LoadTypeAsModule(this ILLoader loader, Type type)
            => new Objects.TypeAsModule(type, loader);
    }
}
