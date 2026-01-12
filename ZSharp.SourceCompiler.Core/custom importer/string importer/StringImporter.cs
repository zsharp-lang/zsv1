using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.SourceCompiler
{
    public sealed class StringImporter
        : IStringImporter
    {
        private readonly Mapping<string, IStringImporter> customStringImporters = [];

        public IStringImporter? DefaultImporter { get; set; }

        public IResult Import(string source)
        {
            var parts = source.Split(':', count: 2, options: StringSplitOptions.TrimEntries);
            if (parts.Length > 1)
            {
                if (!customStringImporters.TryGetValue(parts[0], out var importer))
                    return Result<CompilerObject>.Error(
                        $"No string importer registered for prefix '{parts[0]}'."
                    );

                return importer.Import(parts[1]);
            }

            if (parts.Length == 0)
                return Result<CompilerObject>.Error("Source string is empty.");

            if (DefaultImporter is null)
                return Result<CompilerObject>.Error("No default string importer is set.");

            if (DefaultImporter == this)
                return Result<CompilerObject>.Error("Default string importer cannot be itself.");

            return DefaultImporter.Import(parts[0]);
        }

        public void RegisterImporter(string prefix, IStringImporter importer)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Prefix cannot be null or whitespace.", nameof(prefix));
            if (importer is null)
                throw new ArgumentNullException(nameof(importer), "Importer cannot be null.");
            customStringImporters[prefix] = importer;
        }
    }
}
