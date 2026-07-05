namespace ZSharp.Compiler
{
    public delegate IResult GetIndex(CompilerObject @object, Argument[] arguments);
    public delegate IResult SetIndex(CompilerObject @object, Argument[] arguments, CompilerObject value);

    partial class CG
    {
        public GetIndex GetIndex { get; set; } = Dispatcher.Index;

        public SetIndex SetIndex { get; set; } = Dispatcher.Index;

        public IResult Index(CompilerObject @object, Argument[] arguments)
            => GetIndex(@object, arguments);

        public IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
            => SetIndex(@object, arguments, value);
    }
}
