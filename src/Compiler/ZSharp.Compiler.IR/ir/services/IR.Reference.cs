namespace ZSharp.Compiler
{
    partial class IR
    {
        public IIRReferenceCompiler ReferenceCompiler { get; set; } = Dispatcher.Instance;

        /// <summary>
        /// Gets what code uses to refer to an object, which may be something the
        /// object does not define, such as a handle.
        /// </summary>
        public IResult<T, Error> CompileReference<T>(CompilerObject @object, object? target)
            where T : class
            => ReferenceCompiler.CompileReference<T>(@object, target);
    }
}
