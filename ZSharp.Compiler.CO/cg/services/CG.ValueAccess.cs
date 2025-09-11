namespace ZSharp.Compiler
{
    public delegate Result Get(CompilerObject @object);

    public delegate Result Set(CompilerObject @object, CompilerObject value);

    public partial struct CG
    {
        public Get Get { get; set; } = Dispatcher.Get;

        public Set Set { get; set; } = Dispatcher.Set;
    }
}
