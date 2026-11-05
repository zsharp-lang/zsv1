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

        public IResult<HIR.Instance, Error> Import(string source)
        {
            var parts = source.Split(':', count: 2, StringSplitOptions.TrimEntries);

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
