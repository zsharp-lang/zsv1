namespace ZSharp.Compiler
{
    public delegate IResult Get(CompilerObject @object);

    public delegate IResult Set(CompilerObject @object, CompilerObject value);

    public partial class CG
    {
        public Get Get { get; set; } = Dispatcher.Get;

        public Set Set { get; set; } = Dispatcher.Set;
    }
}
