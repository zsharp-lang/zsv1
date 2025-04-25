namespace ZSharp.Compiler
{
    public interface ICTTypeCast
    {
        public CompilerObject Cast(Compiler compiler, IType targetType);
    }

    public interface IRTTypeCast
    {
        public CompilerObject Cast(Compiler compiler, CompilerObject @object, IType targetType);
    }
}
