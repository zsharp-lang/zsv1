namespace ZSharp.SourceCompiler
{
    public interface IReflectionPlatform
    {
        public CompilerObject CreateSlot();

        public void AddDeclaration(CompilerObject code);

        public void AddDefinition(CompilerObject code);
    }
}
