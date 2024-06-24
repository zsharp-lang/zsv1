using CommonZ.Utils;
using ZSharp.VM;

namespace ZSharp.IR.Standard
{
    public sealed class StringImporter : IStringImporter
    {
        private readonly Mapping<string, IStringImporter> importers = new();

        public IStringImporter? DefaultImporter { get; set; }

        public string Separator { get; set; } = ":";

        public void AddImporter(string type, IStringImporter importer)
        {
            importers.Add(type, importer);
        }

        public IStringImporter GetImporter(string type)
        {
            return importers[type];
        }

        public ZSObject Import(string source)
        {
            string[] parts = source.Split(Separator, 2);

            if (parts.Length == 1)
            {
                if (DefaultImporter is null) throw new Exception();
                return DefaultImporter.Import(source);
            }

            string type = parts[0];
            source = parts[1];

            return GetImporter(type).Import(source);
        }
    }
}
