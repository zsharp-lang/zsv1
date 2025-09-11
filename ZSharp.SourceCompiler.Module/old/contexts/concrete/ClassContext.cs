using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class ClassContext(
        Objects.Class @class
    )
        : IContext
        , IObjectContext<Objects.Class>
    {
        public IContext? Parent { get; set; }

        public Objects.Class Class { get; } = @class;

        Objects.Class IObjectContext<Objects.Class>.Object => Class;
    }
}
