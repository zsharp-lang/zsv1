namespace ZSharp.Compiler
{
    public delegate Result Call(CompilerObject callee, Argument[] arguments);

    partial struct CG
    {
        public Call Call { get; set; } = Dispatcher.Call;
    }
}
