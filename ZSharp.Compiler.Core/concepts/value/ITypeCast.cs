namespace ZSharp.Compiler
{
    public interface ICTTypeCast
    {
        public CTObject Cast(ZSCompiler compiler, CTObject targetType);
    }

    public interface IRTTypeCast
    {
        public CTObject Cast(ZSCompiler compiler, CTObject @object, CTObject targetType);
    }
}
