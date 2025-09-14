namespace ZSharp.Compiler
{
    public delegate Result<IRCode> CompileIRCode(CompilerObject @object, TargetPlatform? target);

    partial struct IR
    {
        public CompileIRCode CompileCode { get; set; } = Dispatcher.Instance.CompileCode;
    }
}
