namespace ZSharp.Compiler
{
    public delegate IResult ImplicitCast(CompilerObject @object, CompilerObject type);

    partial class CG
    {
        public ImplicitCast ImplicitCast { get; set; } = Dispatcher.ImplicitCast;
    }
}
