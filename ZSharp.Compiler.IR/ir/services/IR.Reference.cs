namespace ZSharp.Compiler
{
    partial struct IR
    {
        public IIRReferenceCompiler ReferenceCompiler { get; set; } = Dispatcher.Instance;

        public IResult<T, Error> CompileReference<T>(CompilerObject @object)
            where T : class
            => ReferenceCompiler.CompileReference<T>(@object);
    }
}
