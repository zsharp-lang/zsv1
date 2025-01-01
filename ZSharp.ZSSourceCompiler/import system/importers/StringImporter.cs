using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class StringImporter
        : CompilerObject
        , ICTCallable
    {
        public Mapping<string, CompilerObject> Importers { get; } = [];

        public CompilerObject? DefaultImporter { get; set; }

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            Argument sourceArgument = arguments.FirstOrDefault(arg => arg.Name is null) ?? throw new();

            if (!compiler.IsString(compiler.Evaluate(sourceArgument.Object), out var source))
                throw new(); // TODO: proper exception: first parameter must be a string

            string[] parts = source.Split(':', 2);
            if (parts.Length == 1)
                if (DefaultImporter is null)
                    throw new(); // TODO: proper exception
                else return compiler.Call(DefaultImporter, arguments);

            string prefix = parts[0];
            source = parts[1];

            if (!Importers.TryGetValue(prefix, out var importer))
                throw new(); // TODO: proper exception

            sourceArgument.Object = compiler.CreateString(source);

            return compiler.Call(importer, arguments);
        }
    }
}
