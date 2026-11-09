namespace ZSharp.Importer.String
{
    /// <summary>
    /// Routes a source such as <c>std:io</c> to the importer of its prefix.
    /// </summary>
    public sealed class StringImporter
        : IStringImporter
    {
        private readonly Dictionary<string, IStringImporter> importers = [];

        public IStringImporter? DefaultImporter { get; set; }

        public void Register(string prefix, IStringImporter importer)
            => importers[prefix] = importer;

        /// <summary>
        /// Routes a prefix to a function, which is given the rest of the source, as
        /// <c>pkg:</c> is to the package manager's.
        /// </summary>
        public void Register(string prefix, Func<string, IResult<HIR.Instance, Error>> import)
            => importers[prefix] = new FunctionImporter(import);

        private sealed class FunctionImporter(Func<string, IResult<HIR.Instance, Error>> import)
            : IStringImporter
        {
            public IResult<HIR.Instance, Error> Import(string source) => import(source);
        }

        public IResult<HIR.Instance, Error> Import(string source)
        {
            // A rooted path, such as C:/a.zs, has no prefix.
            var parts = Path.IsPathRooted(source)
                ? [source]
                : source.Split(':', count: 2, StringSplitOptions.TrimEntries);

            if (parts.Length > 1)
                return importers.TryGetValue(parts[0], out var importer)
                    ? importer.Import(parts[1])
                    : Result<HIR.Instance>.Error($"No importer is registered for the prefix '{parts[0]}'.");

            if (DefaultImporter is null)
                return Result<HIR.Instance>.Error($"No default importer is set to import '{source}'.");

            return DefaultImporter.Import(source);
        }
    }
}
