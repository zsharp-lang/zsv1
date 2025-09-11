using ZSharp.Compiler;
using ZSharp.Objects;

namespace ZSharp.SourceCompiler
{
    public interface IStringImporter
    {
        public Result<CompilerObject> Import(string source);
    }
}
