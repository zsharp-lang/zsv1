namespace ZSharp.Compiler
{
    public delegate IResult Call(CompilerObject callee, Argument[] arguments);

    partial class CG
    {
        public Call Call { get; set; } = Dispatcher.Call;
    }
}
