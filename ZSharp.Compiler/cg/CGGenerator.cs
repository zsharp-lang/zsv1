using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed class CGGenerator
    {
        private readonly CGCompiler.Compiler compiler = new();

        public CGObjects.Module Compile(RStatement[] statements, string? moduleName = null)
        {
            return compiler.Compile(new(moduleName ?? string.Empty)
            {
                Content = new RBlock([.. statements])
            });
        }
    }
}
