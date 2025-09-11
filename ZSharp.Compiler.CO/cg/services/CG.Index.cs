namespace ZSharp.Compiler
{
    public delegate Result GetIndex(CompilerObject @object, Argument[] arguments);
    public delegate Result SetIndex(CompilerObject @object, Argument[] arguments, CompilerObject value);

    partial struct CG
    {
        public GetIndex GetIndex { get; set; } = Dispatcher.Index;

        public SetIndex SetIndex { get; set; } = Dispatcher.Index;

        public readonly Result Index(CompilerObject @object, Argument[] arguments)
            => GetIndex(@object, arguments);

        public readonly Result Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
            => SetIndex(@object, arguments, value);
    }
}
