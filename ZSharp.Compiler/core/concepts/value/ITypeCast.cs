namespace ZSharp.Compiler
{
    public interface ICTTypeCast
    {
        public CompilerObject Cast(Compiler compiler, CompilerObject targetType);
    }

    public interface IRTTypeCast
    {
        public CompilerObject Cast(Compiler compiler, CompilerObject @object, CompilerObject targetType);
    }
}
