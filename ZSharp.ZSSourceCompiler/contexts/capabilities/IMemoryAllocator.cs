using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public interface IMemoryAllocator
        : IContext
    {
        public CompilerObject Allocate(
            string name,
            IType type,
            CompilerObject? initializer = null
        );
    }
}
