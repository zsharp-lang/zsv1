namespace ZSharp.Compiler
{
    public delegate Result ImplicitCast(CompilerObject @object, CompilerObject type);

    partial struct CG
    {
        public ImplicitCast ImplicitCast { get; set; } = Dispatcher.ImplicitCast;
    }
}
