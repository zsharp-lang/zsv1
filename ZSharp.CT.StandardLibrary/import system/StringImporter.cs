using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CT.StandardLibrary
{
    public sealed class StringImporter : BasicImporter<string>
    {
        private readonly Mapping<string, BasicImporter<string>> importers = [];

        public BasicImporter<string>? DefaultImporter { get; set; }

        public void AddImporter(string kind, BasicImporter<string> importer)
        {
            importers.Add(kind, importer);
        }

        protected internal override CGObject Import(string source, Argument[] arguments)
        {
            var parts = source.Split(':', 2);

            if (parts.Length != 2)
                return DefaultImporter?.Import(source, arguments) ?? throw new();

            var kind = parts[0];
            source = parts[1];

            if (!importers.TryGetValue(kind, out var importer))
                throw new();

            return importer.Import(source, arguments);
        }
    }
}
