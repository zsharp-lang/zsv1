namespace ZSharp.Compiler
{
    partial struct IR
    {
        public IIRReferenceCompiler ReferenceCompiler { get; set; } = Dispatcher.Instance;

        public Result<T> CompileReference<T>(CompilerObject @object)
            where T : class
            => ReferenceCompiler.CompileReference<T>(@object);
    }
}
