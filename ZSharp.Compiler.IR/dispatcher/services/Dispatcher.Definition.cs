namespace ZSharp.Compiler
{
    partial class Dispatcher
        : IIRDefinitionInCompiler
        , IIRDefinitionAsCompiler
    {
        public bool CompileDefinition<Owner>(CompilerObject @object, Owner owner, TargetPlatform? target)
            where Owner : IRDefinition
            => false;

        public Result<T> CompileDefinition<T>(CompilerObject @object, object? target) where T : IRDefinition
            => Result<T>.Error(
                $"Object {@object} does not support compile IR definition of type {typeof(T)}"
            );
    }
}
