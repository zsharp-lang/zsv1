namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RImport Resolve(ImportStatement import)
            => new(Resolve(import.Source))
            {
                Arguments = null,
                As = import.Alias is null ? null : new(import.Alias),
                Targets =
                    import.ImportedNames is null
                    ? null
                    : [.. import.ImportedNames.Select(Resolve)]
            };

        public static RImportTarget Resolve(ImportedName name)
            => new(name.Name, name.Alias is null ? null : new(name.Alias));


    }
}
