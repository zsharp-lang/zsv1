namespace ZSharp.Compiler
{
    partial struct IR
    {
        public IIRDefinitionAsCompiler DefinitionAsCompiler { get; set; } = Dispatcher.Instance;

        public IIRDefinitionInCompiler DefinitionInCompiler { get; set; } = Dispatcher.Instance;

        public Result<T> CompileDefinition<T>(CompilerObject @object, TargetPlatform? target)
            where T : IRDefinition
            => DefinitionAsCompiler.CompileDefinition<T>(@object, target);

        public bool CompileDefinition<Owner>(CompilerObject @object, Owner owner, TargetPlatform? target)
            where Owner : IRDefinition
            => DefinitionInCompiler.CompileDefinition(@object, owner, target);
    }
}
