namespace ZSharp.Compiler
{
    public delegate IResult<IRCode, Error> CompileIRCode(CompilerObject @object, TargetPlatform? target);

    partial struct IR
    {
        public CompileIRCode CompileCode { get; set; } = Dispatcher.Instance.CompileCode;
    }
}
