namespace ZSharp.Compiler
{
    public delegate IResult Cast(CompilerObject @object, CompilerObject type);

    partial class CG
    {
        public Cast Cast { get; set; } = Dispatcher.Cast;
    }
}
