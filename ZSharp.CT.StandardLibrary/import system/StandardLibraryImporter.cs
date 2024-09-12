using CommonZ.Utils;
using ZSharp.CGObjects;
using ZSharp.Compiler;

namespace ZSharp.CT.StandardLibrary
{
    public sealed class StandardLibraryImporter : BasicImporter<string>
    {
        private readonly Mapping<string, Module> modules = [];

        public void AddModule(Module module, string? name = null)
            => modules.Add(name ?? module.Name ?? throw new(), module);

        protected internal override CGObject Import(string source, Argument[] arguments)
        {
            if (!modules.TryGetValue(source, out var module))
                throw new();

            return module;
        }
    }
}
