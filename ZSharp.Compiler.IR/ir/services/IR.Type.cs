namespace ZSharp.Compiler
{
    public delegate Result<IType> CompileType(CompilerObject @object);

    partial struct IR
    {
        public CompileType CompileType { get; set; } = Dispatcher.Instance.CompileType;

        public IIRTypeCompiler TypeCompiler { get; set; } = Dispatcher.Instance;

        public Result<T> CompileTypeAs<T>(CompilerObject @object)
            where T : class, IType
            => TypeCompiler.CompileType<T>(@object);
    }
}
