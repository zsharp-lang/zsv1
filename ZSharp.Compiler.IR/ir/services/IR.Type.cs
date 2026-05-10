namespace ZSharp.Compiler
{
    public delegate IResult<IType, Error> CompileType(CompilerObject @object, object? target);

    partial class IR
    {
        public CompileType CompileType { get; set; } = Dispatcher.Instance.CompileType;

        public IIRTypeCompiler TypeCompiler { get; set; } = Dispatcher.Instance;

        public IResult<T, Error> CompileTypeAs<T>(CompilerObject @object, object? target)
            where T : class, IType
            => TypeCompiler.CompileType<T>(@object, target);
    }
}
