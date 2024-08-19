namespace ZSharp.CTRuntime
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
