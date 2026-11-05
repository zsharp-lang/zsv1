namespace ZSharp.Importer.String
{
    /// <summary>
    /// A named set of modules, such as those behind <c>std:</c> or <c>core:</c>.
    /// </summary>
    public sealed class LibraryImporter(string name)
        : IStringImporter
    {
        private readonly Dictionary<string, HIR.Instance> modules = [];

        public void Add(string name, HIR.Instance module)
            => modules[name] = module;

        public IResult<HIR.Instance, Error> Import(string source)
            => modules.TryGetValue(source, out var module)
                ? Result<HIR.Instance>.Ok(module)
                : Result<HIR.Instance>.Error($"Could not find {name} library '{source}'.");
    }
}
