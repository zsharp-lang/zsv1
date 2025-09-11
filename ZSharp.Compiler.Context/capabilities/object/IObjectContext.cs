namespace ZSharp.Compiler
{
    public interface IObjectContext
        : IContext
    {
        public CompilerObject Object { get; }
    }

    public interface IObjectContext<out T>
        : IObjectContext
        where T : CompilerObject
    {
        new public T Object { get; }

        CompilerObject IObjectContext.Object => Object;
    }
}
