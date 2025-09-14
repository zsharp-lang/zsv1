namespace ZSharp.Compiler
{
    public delegate Result F(CompilerObject callee);

    partial struct Overloading
    {
        public F CaFll { get; set; } = Dispatcher.F;
    }
}
