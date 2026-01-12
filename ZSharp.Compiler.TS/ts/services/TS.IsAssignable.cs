namespace ZSharp.Compiler
{
    public delegate bool IsAssignableTo(CompilerObject target, CompilerObject source);

    partial struct TS
    {
        public IsAssignableTo IsAssignableTo { get; set; } = Dispatcher.IsAssignableTo;

        public bool IsAssignableFrom(CompilerObject source, CompilerObject target)
            => IsAssignableTo(target, source);
    }
}
