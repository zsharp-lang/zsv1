namespace ZSharp.Compiler
{
    partial class IR
    {
        public IIRDefinitionAsCompiler DefinitionAsCompiler { get; set; } = Dispatcher.Instance;

        public IIRDefinitionInCompiler DefinitionInCompiler { get; set; } = Dispatcher.Instance;

        /// <summary>
        /// Gets the entity an object defines, without placing it anywhere.
        /// </summary>
        public IResult<T, Error> CompileDefinition<T>(CompilerObject @object, object? target)
            where T : LIR.Definition
            => DefinitionAsCompiler.CompileDefinition<T>(@object, target);

        /// <summary>
        /// Places the entity an object defines into an owner.
        /// </summary>
        /// <returns>
        /// <see langword="null"/> if it was placed, otherwise why it was not.
        /// </returns>
        public Error? CompileDefinition<Owner>(CompilerObject @object, Owner owner, object? target)
            where Owner : LIR.Definition
            => DefinitionInCompiler.CompileDefinition(@object, owner, target);
    }
}
