namespace ZSharp.Importer.String
{
    /// <summary>
    /// A named set of modules, such as those behind <c>std:</c> or <c>core:</c>.
    /// </summary>
    public sealed class LibraryImporter(string name)
        : IStringImporter
    {
        private readonly Dictionary<string, HIR.Instance> modules = [];

        /// <summary>
        /// What imports a library this one has no module for, if anything does.
        /// </summary>
        public IStringImporter? Fallback { get; set; }

        public void Add(string name, HIR.Instance module)
            => modules[name] = module;

        public IResult<HIR.Instance, Error> Import(string source)
            => modules.TryGetValue(source, out var module)
                ? Result<HIR.Instance>.Ok(module)
                : Fallback is not null
                    ? Fallback.Import(source)
                    : Result<HIR.Instance>.Error($"Could not find {name} library '{source}'.");
    }
}
