namespace ZSharp.Compiler
{
    public delegate IResult<LIR.Type, Error> CompileType(CompilerObject @object, object? target);

    partial class IR
    {
        public CompileType CompileType { get; set; } = Dispatcher.Instance.CompileType;
    }
}
