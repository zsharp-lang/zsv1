namespace ZSharp.Compiler
{
    public delegate IResult F(CompilerObject callee);

    partial struct Overloading
    {
        public F CaFll { get; set; } = Dispatcher.F;
    }
}
