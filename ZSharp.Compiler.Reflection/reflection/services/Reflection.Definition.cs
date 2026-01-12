namespace ZSharp.Compiler
{
    public delegate bool AreSameDefinition(CompilerObject left, CompilerObject right);

    partial struct Reflection
    {
        public AreSameDefinition IsSameDefinition { get; set; } = Dispatcher.IsSameDefinition;
    }
}
