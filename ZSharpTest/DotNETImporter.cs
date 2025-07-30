using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp
{
    public sealed class DotNETImporter(Interpreter.Interpreter interpreter)
        : Objects.CompilerObject
        , ICTCallable_Old
    {
        private readonly Interpreter.Interpreter interpreter = interpreter;

        public Mapping<string, Objects.CompilerObject> Libraries { get; } = [];

        public Objects.CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (arguments.Length == 0)
                throw new(); // TODO: proper exception

            if (
                arguments.Length > 1 ||
                arguments[0].Name is not null ||
                interpreter.Evaluate(arguments[0].Object).Unwrap() is not string libraryName
            )
            {
                compiler.Log.Error("`net` importer must have exactly 1 argument of type `string`", this);
                throw new(); // TODO: proper exception
            }

            if (!Libraries.TryGetValue(libraryName, out var library))
                library = Libraries[libraryName] = ImportDotNETLibrary(libraryName);

            return library;
        }

        private Objects.CompilerObject ImportDotNETLibrary(string libraryName)
        {
            var assembly = System.Reflection.Assembly.LoadFile(Path.GetFullPath(libraryName));

            var modules = assembly.GetModules();
            if (modules.Length != 1)
            {
                interpreter.Compiler.Log.Error($"Assembly {libraryName} has more than 1 module", this);
                throw new(); // TODO: proper exception
            }

            var module = modules[0];

            return interpreter.ImportILModule(module);
        }
    }
}
