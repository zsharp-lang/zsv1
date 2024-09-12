using ZSharp.CGObjects;
using ZSharp.Compiler;

namespace ZSharp.CT.StandardLibrary
{
    public sealed class ImportFunction(StringImporter importer) : CTFunction("import")
    {
        private readonly StringImporter importer = importer;

        public override CGObject Call(ICompiler compiler, Argument[] arguments)
        {
            if (arguments.Length == 0) throw new();

            var sourceArgument = arguments[0];

            if (sourceArgument.Name is not null) throw new();

            arguments = arguments.Skip(1).ToArray();

            if (!compiler.IsCTValue(sourceArgument.Object, out string? source))
                throw new NotImplementedException();

            return importer.Import(source, arguments);
        }

        public override OverloadMatchResult? Match(ICompiler compiler, Argument[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
