namespace ZSharp.Compiler
{
    partial struct IR
    {
        public IIRReferenceCompiler ReferenceCompiler { get; set; } = Dispatcher.Instance;

        public readonly IResult<T, Error> CompileReference<T>(CompilerObject @object, object? target)
            where T : class
            => ReferenceCompiler.CompileReference<T>(@object, target);
    }
}
