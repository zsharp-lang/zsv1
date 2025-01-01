using CommonZ.Utils;
using System.Data;
using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class StandardLibraryImporter
        : CompilerObject
        , ICTCallable
    {
        public Mapping<string, CompilerObject> Libraries { get; } = [];

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (arguments.Length == 0)
                throw new(); // TODO: proper exception

            if (
                arguments.Length > 1 ||
                arguments[0].Name is not null ||
                !compiler.IsString(compiler.Evaluate(arguments[0].Object), out var libraryName)
            )
            {
                compiler.Log.Error("`std` importer must have exactly 1 argument of type `string`", this);
                throw new(); // TODO: proper exception
            }

            if (!Libraries.TryGetValue(libraryName, out var library))
                throw new(); // TODO: proper exception

            return library;
        }
    }
}
