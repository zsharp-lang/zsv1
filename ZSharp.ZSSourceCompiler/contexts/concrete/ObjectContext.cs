using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public class ObjectContext<T>(T @object)
        : IObjectContext<T>
        where T : CompilerObject
    {
        public IContext? Parent { get; set; }

        public T Object { get; } = @object;
    }
}
