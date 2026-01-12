using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public interface IObjectContext
        : IContext
    {
        public CompilerObject Object { get; }
    }

    public interface IObjectContext<T>
        : IContext
        , IObjectContext
        where T : CompilerObject
    {
        public new T Object { get; }

        CompilerObject IObjectContext.Object => Object;
    }
}
